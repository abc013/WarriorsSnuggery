using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Parts;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public sealed class Actor : PhysicsObject
	{
		public const byte PlayerTeam = 2;
		public const byte NeutralTeam = 0;

		public readonly World World;

		public readonly bool IsPlayer;
		public readonly bool IsBot;
		public readonly bool IsPlayerSwitch;
		public readonly uint ID;

		public bool IsAlive = true;

		public ActorSector Sector;

		// Used when changing player for bot targeting.
		public Actor FollowupActor;

		[Save]
		public byte Team;
		[Save]
		public float Angle;

		public readonly List<ActorPart> Parts = new List<ActorPart>();
		public readonly List<EffectPart> Effects = new List<EffectPart>();
		public readonly PartManager PartManager;

		readonly List<ITick> tickParts;
		readonly List<ITickInEditor> editorTickParts;
		readonly List<IRenderable> renderParts;
		readonly List<INoticeAcceleration> accelerationParts;
		readonly List<INoticeMove> moveParts;
		readonly List<INoticeStop> stopParts;

		public readonly MobilityPart Mobility;
		public readonly HealthPart Health;

		public readonly RevealsShroudPart RevealsShroudPart;

		public WeaponPart ActiveWeapon;

		public readonly WorldPart WorldPart;

		public readonly BotPart BotPart;

		[Save]
		public readonly ActorType Type;
		ActorInit init;

		public MPos TerrainPosition;
		public Terrain CurrentTerrain;

		bool visible;

		CPos Velocity
		{
			get => Mobility == null ? CPos.Zero : Mobility.Velocity;
			set
			{
				if (Mobility != null)
					Mobility.Velocity = value;
			}
		}

		// TODO: needs to be saved
		ActorAction upcoming;
		public ActorAction CurrentAction = ActorAction.Default;

		public Actor(World world, ActorInit init, uint overrideID) : this(world, init)
		{
			ID = overrideID;
		}

		public Actor(World world, ActorInit init) : base(init.Position, null)
		{
			World = world;

			Type = init.Type;
			Team = init.Team;
			IsPlayer = init.IsPlayer;
			IsBot = init.IsBot;

			Height = init.Height;

			ID = init.ID;
			this.init = init;

			TerrainPosition = init.Position.ToMPos();
			CurrentTerrain = world.TerrainAt(TerrainPosition);

			// Parts
			foreach (var partinfo in init.Type.PartInfos)
				Parts.Add(partinfo.Create(this));

			Mobility = (MobilityPart)Parts.Find(p => p is MobilityPart);

			Health = (HealthPart)Parts.Find(p => p is HealthPart);
			if (Health != null && init.Health >= 0f)
				Health.RelativeHP = init.Health;

			RevealsShroudPart = (RevealsShroudPart)Parts.Find(p => p is RevealsShroudPart);

			ActiveWeapon = (WeaponPart)Parts.Find(p => p is WeaponPart);

			WorldPart = (WorldPart)Parts.Find(p => p is WorldPart);
			if (WorldPart != null)
				Height = WorldPart.Height;

			IsPlayerSwitch = Parts.Any(p => p is PlayerSwitchPart);

			if (IsPlayer)
				Parts.Add(new PlayerPart(this));

			if (IsBot)
			{
				var behavior = WorldPart == null ? Bot.BotBehaviorType.TYPICAL : WorldPart.BotBehavior;
				BotPart = new BotPart(this, behavior);
				Parts.Add(BotPart);
			}

			PartManager = new PartManager();
			foreach (var part in Parts)
				PartManager.Add(part);

			tickParts = PartManager.GetOrDefault<ITick>();
			editorTickParts = PartManager.GetOrDefault<ITickInEditor>();
			renderParts = PartManager.GetOrDefault<IRenderable>();
			accelerationParts = PartManager.GetOrDefault<INoticeAcceleration>();
			moveParts = PartManager.GetOrDefault<INoticeMove>();
			stopParts = PartManager.GetOrDefault<INoticeStop>();

			Physics = getPhysics(init.Type);
		}

		SimplePhysics getPhysics(ActorType type)
		{
			if (type.Physics == null)
				return SimplePhysics.Empty;

			return new SimplePhysics(this, type.Physics.Type);
		}

		public void OnLoad()
		{
			foreach (var part in Parts)
				part.OnLoad(init.Nodes);

			var effects = init.Nodes.Where(n => n.Key == nameof(EffectPart));
			foreach (var effect in effects)
				Effects.Add(new EffectPart(this, effect.Children));

			init = null;
		}

		public List<string> Save()
		{
			var list = WorldSaver.GetSaveFields(this);

			foreach (var part in Parts)
				list.AddRange(part.OnSave().GetSave());

			foreach (var part in Effects)
				list.AddRange(part.Save());

			return list;
		}

		public void Push(float angle, int power)
		{
			if (Mobility == null || World.Game.Editor)
				return;

			var acceleration = Mobility.Accelerate(angle, power);
			foreach (var part in accelerationParts)
				part.OnAccelerate(angle, acceleration);
		}

		public void AccelerateSelf(float angle)
		{
			if (Mobility == null || World.Game.Editor)
				return;

			if (!canMove())
				return;

			var acceleration = Mobility.AccelerateSelf(angle);
			foreach (var part in accelerationParts)
				part.OnAccelerate(angle, acceleration);
		}

		public void Lift(int power)
		{
			if (Mobility == null || World.Game.Editor)
				return;

			var acceleration = Mobility.AccelerateHeight(power);
			foreach (var part in accelerationParts)
				part.OnAccelerate(new CPos(0, 0, acceleration));
		}

		public void AccelerateHeightSelf(bool up)
		{
			if (Mobility == null || World.Game.Editor)
				return;

			if (!Mobility.CanFly || !canMove())
				return;

			var acceleration = Mobility.AccelerateHeightSelf(up);
			foreach (var part in accelerationParts)
				part.OnAccelerate(new CPos(0, 0, acceleration));
		}

		bool canMove()
		{
			if (!IsAlive || Height > 0 && !Mobility.CanFly)
				return false;

			if ((ActiveWeapon == null || !ActiveWeapon.AllowMoving) && (CurrentAction.Type == ActionType.END_ATTACK || CurrentAction.Type == ActionType.ATTACK))
				return false;

			if (Effects.Any(e => e.Active && e.Effect.Type == Spells.EffectType.STUN))
				return false;

			return true;
		}

		public void Move(CPos old)
		{
			foreach (var part in moveParts)
				part.OnMove(old, Velocity);
		}

		public void StopMove()
		{
			foreach (var part in stopParts)
				part.OnStop();
		}

		public void CastSpell(Spells.Spell spell)
		{
			if (World.Game.Editor)
				return;

			if (spell.Sound != null)
			{
				var sound = new Sound(spell.Sound);
				sound.Play(Position, false);
			}

			for (int i = 0; i < spell.Effects.Length; i++)
				Effects.Add(new EffectPart(this, spell.Effects[i], spell, i));
		}

		public override bool CheckVisibility()
		{
			if (Disposed)
				return false;

			if (WorldPart != null)
				visible = VisibilitySolver.IsVisible(GraphicPosition + WorldPart.VisibilityBoxOffset, WorldPart.VisibilityBox);
			else
				visible = VisibilitySolver.IsVisible(GraphicPosition, new MPos(512, 512));

			return visible;
		}

		public override void Render()
		{
			if (!visible)
				return;

			if (Effects.Any(e => e.Active && e.Effect.Type == Spells.EffectType.INVISIBILITY))
				return;

			base.Render();

			foreach (var part in renderParts)
				part.Render();
		}

		public void RenderDebug()
		{
			Graphics.ColorManager.DrawDot(Position, Color.Blue);
			if (ActiveWeapon != null)
				Graphics.ColorManager.DrawCircle(Position, ActiveWeapon.Type.MaxRange / 1024f * 2, Color.Red);
		}

		public override void Tick()
		{
			base.Tick();

			if (!IsAlive)
				return;

			if (World.Game.Editor)
			{
				foreach (var part in editorTickParts)
					part.Tick();

				return;
			}

			foreach (var part in tickParts)
				part.Tick();

			if (CurrentAction.Tick())
			{
				if (upcoming != null)
				{
					CurrentAction = upcoming;
					upcoming = null;
				}
				else
				{
					CurrentAction = ActorAction.Default;
				}
			}

			var effectsToRemove = new List<EffectPart>();
			foreach (var effect in Effects)
			{
				effect.Tick();
				if (!effect.Active)
					effectsToRemove.Add(effect);
			}
			foreach(var effect in effectsToRemove)
				Effects.Remove(effect);
		}

		public bool QueueAction(ActorAction action)
		{
			if (action.ActionOver)
				return true;

			if (CurrentAction.ActionOver)
			{
				CurrentAction = upcoming ?? action;

				if (upcoming == null)
					return true;

				upcoming = null;
			}

			if (upcoming != null)
				return false;

			upcoming = action;
			return true;
		}

		public bool SetAction(ActorAction action)
		{
			if (action.ActionOver)
				return true;

			if (CurrentAction.Skippable || CurrentAction.ActionOver)
			{
				CurrentAction = action;
				return true;
			}

			return false;
		}

		public override void SetColor(Color color)
		{
			foreach (var part in Parts.Where(p => p is RenderablePart))
				((RenderablePart)part).SetColor(color);
		}

		public void PrepareAttack(Actor target)
		{
			PrepareAttack(new Target(target));
		}

		public void PrepareAttack(CPos target, int height)
		{
			PrepareAttack(new Target(target, height));
		}

		public void PrepareAttack(Target target)
		{
			if (ActiveWeapon == null || !ActiveWeapon.ReloadDone || !IsAlive)
				return;

			if (World.Game.Editor && IsPlayer || !World.Map.Type.AllowWeapons)
				return;

			if (CurrentAction.Type == ActionType.PREPARE_ATTACK)
				return;

			if (Effects.Any(e => e.Active && e.Effect.Type == Spells.EffectType.STUN))
				return;

			if (!SetAction(new ActorAction(ActionType.PREPARE_ATTACK, true, ActiveWeapon.Type.PreparationDelay)))
				return;

			Angle = (Position - target.Position).FlatAngle;

			ActiveWeapon.OnAttack(target);
		}

		public bool AttackWith(Target target, Weapon weapon)
		{
			if (!World.Map.Type.AllowWeapons)
				return false;

			World.Add(weapon);

			SetAction(new ActorAction(ActionType.ATTACK, false, ActiveWeapon.Type.ShootDuration));
			QueueAction(new ActorAction(ActionType.END_ATTACK, false, ActiveWeapon.Type.CooldownDelay));

			foreach (var part in PartManager.GetOrDefault<INoticeAttack>())
				part.OnAttack(target.Position, weapon);

			return true;
		}

		public void Kill(Actor killed)
		{
			foreach (var part in PartManager.GetOrDefault<INoticeKill>())
				part.OnKill(killed);
		}

		public void Damage(Actor attacker, int damage)
		{
			if (World.Game.Editor)
				return;

			if (Health == null || Health.HP <= 0)
				return;

			if (Effects.Any(e => e.Active && e.Effect.Type == Spells.EffectType.SHIELD))
				return;

			Health.HP -= damage;

			foreach (var part in PartManager.GetOrDefault<INoticeDamage>())
				part.OnDamage(attacker, damage);

			if (Health.HP <= 0)
				Killed(attacker);
		}

		public void Damage(int damage)
		{
			Damage(null, damage);
		}

		public void Killed(Actor killer)
		{
			if (World.Game.Editor)
				return;

			if (killer != null)
				killer.Kill(this);

			IsAlive = false;

			foreach (var part in PartManager.GetOrDefault<INoticeKilled>())
				part.OnKilled(killer);

			Dispose();
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (var part in PartManager.GetOrDefault<INoticeDispose>())
				part.OnDispose();
			World.ActorLayer.Remove(this);
		}
	}
}
