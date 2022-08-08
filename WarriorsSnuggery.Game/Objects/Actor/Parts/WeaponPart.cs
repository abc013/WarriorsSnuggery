using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Adds a weapon to the object.")]
	public class WeaponPartInfo : PartInfo
	{
		[Require, Desc("Name of the weapon.")]
		public readonly WeaponType Type;
		[Desc("Offset of the shoot point relative to the object's center.", "Z-Coordinate will be used for height.")]
		public readonly CPos Offset;
		[Desc("Determines whether to allow moving while firing.")]
		public readonly bool AllowMoving;

		public WeaponPartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new WeaponPart(self, this);
		}
	}

	public class WeaponPart : ActorPart, ITick, INoticeDispose, ISaveLoadable, INoticeMove
	{
		readonly WeaponPartInfo info;
		public readonly WeaponType Type;

		public bool AllowMoving => info.AllowMoving;

		public CPos WeaponOffsetPosition => self.GraphicPositionWithoutHeight + new CPos(info.Offset.X, info.Offset.Y, 0);
		public int WeaponOffsetHeight => self.Height + info.Offset.Z;

		public Target Target;

		enum WeaponState
		{
			READY,
			RELOADING,
			PREPARING,
			ATTACKING,
			COOLDOWN
		}

		WeaponState state;
		int stateTick; // time until state changes

		public bool Ready => WeaponState.READY == state;

		// Needs to be saved for controlling the beam.
		BeamWeapon beam;

		public WeaponPart(Actor self, WeaponPartInfo info) : base(self)
		{
			this.info = info;
			Type = info.Type;
		}

		public void OnLoad(PartLoader loader)
		{
			foreach (var node in loader.GetNodes(typeof(WeaponPart), info.InternalName))
			{
				switch (node.Key)
				{
					case "BeamWeapon":
						var id = node.Convert<int>();
						beam = (BeamWeapon)self.World.WeaponLayer.Weapons.Find(w => w.ID == id);
						break;
					case "StateTick":
						stateTick = node.Convert<int>();
						break;
					case "State":
						state = node.Convert<WeaponState>();
						break;
					case nameof(Target):
						var pos = node.Convert<CPos>();
						Target = new Target(pos, 0);
						break;
				}
			}
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			if (beam != null)
				saver.Add("BeamWeapon", beam.ID, -1);

			saver.Add("StateTick", stateTick, 0);
			saver.Add("State", state, 0);

			if (Target != null)
			{
				// TODO: also support actor targeting
				saver.Add(nameof(Target), Target.Position + new CPos(0, 0, Target.Height), CPos.Zero);
			}
			
			return saver;
		}

		public void OrderAttack(Target target)
		{
			if (state != WeaponState.READY)
				return;

			Target = target;

			self.AddAction(ActionType.PREPARE_ATTACK, Type.PreparationDelay);
			stateTick = Type.PreparationDelay;
			state = WeaponState.PREPARING;
		}

		public void CancelAttack()
		{
			if (state != WeaponState.ATTACKING && state != WeaponState.PREPARING)
				return;

			stateTick = 0;

			if (state == WeaponState.PREPARING)
			{
				self.CancelAction(ActionType.PREPARE_ATTACK);
				// Weapon wasn't fired yet
				state = WeaponState.READY;
			}
			else
			{
				self.CancelAction(ActionType.ATTACK);
			}
		}

		public void Tick()
		{
			if (state == WeaponState.READY)
				return;

			if (stateTick != 0 && state == WeaponState.ATTACKING)
			{
				int salvoIntervals = info.Type.ShootDuration / info.Type.BurstCount;

				if (stateTick % salvoIntervals == 0)
				{
					var weapon = WeaponCache.Create(self.World, info.Type, Target, self);
					beam = weapon as BeamWeapon;

					self.AttackWith(Target, weapon);
				}
			}

			if (stateTick-- == 0)
			{
				switch (state)
				{
					case WeaponState.PREPARING:
						stateTick = info.Type.ShootDuration;
						state = WeaponState.ATTACKING;

						self.AddAction(ActionType.ATTACK, Type.ShootDuration);
						break;
					case WeaponState.ATTACKING:
						stateTick = info.Type.CooldownDelay;
						state = WeaponState.COOLDOWN;

						self.AddAction(ActionType.END_ATTACK, Type.CooldownDelay);
						break;
					case WeaponState.COOLDOWN:
						var reloadModifier = 1f;
						foreach (var effect in self.GetActiveEffects(EffectType.COOLDOWN))
							reloadModifier *= effect.Effect.Value;

						stateTick = (int)(Type.Reload * reloadModifier);
						state = WeaponState.RELOADING;
						break;
					case WeaponState.RELOADING:
						state = WeaponState.READY;
						break;
				}
			}

			if (beam != null)
			{
				if (beam.Disposed)
					beam = null;
				else
					beam.Move(Target.Position, Target.Height);
			}
		}

		public void OnMove(CPos old, CPos speed)
		{
			if (!info.AllowMoving)
				CancelAttack();
		}

		public void OnDispose()
		{
			if (beam != null && !beam.Disposed)
				beam.Dispose();
		}
	}
}
