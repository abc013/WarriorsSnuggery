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
	}

	public class WeaponPart : ActorPart, ITick, INoticeDispose, ISaveLoadable, INoticeMove
	{
		readonly WeaponPartInfo info;
		public readonly WeaponType Type;

		public bool AllowMoving => info.AllowMoving;

		public CPos WeaponOffsetPosition => Self.Position + info.Offset;

		public Target Target;

		enum WeaponState
		{
			READY,
			RELOADING,
			PREPARING,
			ATTACKING,
			COOLDOWN
		}

		[Save("State")]
		WeaponState state;
		[Save("StateTick")]
		int stateTick; // time until state changes

		public bool Ready => WeaponState.READY == state;

		// Needs to be saved for controlling the beam.
		BeamWeapon beam;

		public WeaponPart(Actor self, WeaponPartInfo info) : base(self, info)
		{
			this.info = info;
			Type = info.Type;
		}

		public void OnLoad(PartLoader loader)
		{
			if (loader.ContainsRule(nameof(Target)))
				Target = new Target(loader.MakeInitializerWith(nameof(Target)), Self.World);
			if (loader.ContainsRule("BeamWeapon"))
			{
				var id = loader.Convert<int>("BeamWeapon", int.MaxValue);
				beam = (BeamWeapon)Self.World.WeaponLayer.Weapons.Find(w => w.ID == id);
			}

			loader.SetSaveFields(this);
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this);

			if (beam != null)
				saver.Add("BeamWeapon", beam.ID, -1);
			if (Target != null)
				saver.AddChildren(nameof(Target), Target.Save());

			saver.AddSaveFields(this);

			return saver;
		}

		public void OrderAttack(Target target)
		{
			if (state != WeaponState.READY)
				return;

			Target = target;

			Self.AddAction(ActionType.PREPARE_ATTACK, Type.PreparationDelay);
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
				Self.CancelAction(ActionType.PREPARE_ATTACK);
				// Weapon wasn't fired yet
				state = WeaponState.READY;
			}
			else
			{
				Self.CancelAction(ActionType.ATTACK);
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
					fireWeapon();
			}

			if (stateTick-- == 0)
			{
				switch (state)
				{
					case WeaponState.PREPARING:
						stateTick = info.Type.ShootDuration;
						state = WeaponState.ATTACKING;

						Self.AddAction(ActionType.ATTACK, Type.ShootDuration);

						// Special case: ShootDuration is 0.
						if (Type.ShootDuration == 0)
						{
							for (int i = 0; i < info.Type.BurstCount; i++)
								fireWeapon();
						}
						break;
					case WeaponState.ATTACKING:
						stateTick = info.Type.CooldownDelay;
						state = WeaponState.COOLDOWN;

						Self.AddAction(ActionType.END_ATTACK, Type.CooldownDelay);
						break;
					case WeaponState.COOLDOWN:
						var reloadModifier = 1f;
						foreach (var effect in Self.GetActiveEffects(EffectType.COOLDOWN))
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
					beam.Move(Target.Position);
			}
		}

		void fireWeapon()
		{
			var weapon = WeaponCache.Create(Self.World, info.Type, Target, Self);
			beam = weapon as BeamWeapon;

			Self.AttackWith(Target, weapon);
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
