using System;
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

		public WPos TerrainPosition;
		public Terrain CurrentTerrain;

		int localTick;
		int reloadDelay;

		public bool WeaponReloading => reloadDelay > 0;

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

		ActorAction upcoming;
		public ActorAction CurrentAction = ActorAction.Default;

		public Actor(World world, ActorInit init, uint overrideID) : this(world, init)
		{
			ID = overrideID;
		}

		public Actor(World world, ActorInit init) : base(init.Position, null, getPhysics(init.Position, init.Type))
		{
			World = world;

			Type = init.Type;
			Team = init.Team;
			IsPlayer = init.IsPlayer;
			IsBot = init.IsBot;

			Height = init.Height;

			ID = init.ID;
			this.init = init;

			TerrainPosition = init.Position.ToWPos();
			CurrentTerrain = world.TerrainAt(TerrainPosition);

			// Parts
			foreach (var partinfo in init.Type.PartInfos)
				Parts.Add(partinfo.Create(this));

			Mobility = (MobilityPart)Parts.Find(p => p is MobilityPart);
			Health = (HealthPart)Parts.Find(p => p is HealthPart);

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
			renderParts = PartManager.GetOrDefault<IRenderable>();
			accelerationParts = PartManager.GetOrDefault<INoticeAcceleration>();
			moveParts = PartManager.GetOrDefault<INoticeMove>();
			stopParts = PartManager.GetOrDefault<INoticeStop>();
		}

		static SimplePhysics getPhysics(CPos position, ActorType type)
		{
			if (type.Physics == null)
				return null;

			var info = type.Physics;
			return new SimplePhysics(position, 0, info.Shape, info.Size.X, info.Size.Y, info.Size.Z);
		}

		public void OnLoad()
		{
			foreach (var part in Parts)
				part.OnLoad(init.Nodes);

			var effects = init.Nodes.Where(n => n.Key == "EffectPart");
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

			accelerate(angle, power);
		}

		public void Accelerate(float angle)
		{
			if (Mobility == null || World.Game.Editor || !canMove())
				return;

			accelerate(angle);
		}

		void accelerate(float angle, int customAcceleration = 0)
		{
			var acceleration = Mobility.OnAccelerate(angle, customAcceleration);
			foreach (var part in accelerationParts)
				part.OnAccelerate(angle, acceleration);
		}

		public void Push(int power)
		{
			if (Mobility == null || World.Game.Editor)
				return;

			accelerate(true, power);
		}

		public void AccelerateHeight(bool up)
		{
			if (Mobility == null || World.Game.Editor || !Mobility.CanFly || !canMove())
				return;

			accelerate(up);
		}

		void accelerate(bool up, int customAcceleration = 0)
		{
			var acceleration = Mobility.OnAccelerateHeight(up, customAcceleration);
			foreach (var part in accelerationParts)
				part.OnAccelerate(new CPos(0, 0, acceleration));
		}

		bool canMove()
		{
			if (!IsAlive || Height > 0 && !Mobility.CanFly)
				return false;

			if (CurrentAction.Type == ActionType.END_ATTACK || CurrentAction.Type == ActionType.ATTACK)
				return false;

			if (Effects.Any(e => e.Active && e.Spell.Type == Spells.EffectType.STUN))
				return false;

			return true;
		}

		void move()
		{
			if (!IsAlive || Mobility == null || Velocity == CPos.Zero || World.Game.Editor)
				return;

			var speedModifier = 1f;
			if (Height == 0 && World.TerrainAt(Position) != null)
				speedModifier = World.TerrainAt(Position).Type.Speed;

			if (speedModifier <= 0.01f)
				return;

			var movement = new CPos((int)Math.Round(Velocity.X * speedModifier), (int)Math.Round(Velocity.Y * speedModifier), (int)Math.Round(Velocity.Z * speedModifier));
			if (movement == CPos.Zero)
				return;

			var height = Height + movement.Z;

			// Move in both x and y direction
			var pos = new CPos(Position.X + movement.X, Position.Y + movement.Y, Position.Z);
			if (pos != Position && checkMove(pos, height, Velocity))
				return;

			// Move only in x direction
			if (movement.X != 0)
			{
				var posX = new CPos(Position.X + movement.X, Position.Y, Position.Z);
				if (posX != Position && checkMove(posX, height, new CPos(Velocity.X, 0, Velocity.Z)))
					return;
			}

			// Move only in y direction
			if (movement.Y != 0)
			{
				var posY = new CPos(Position.X, Position.Y + movement.Y, Position.Z);
				if (posY != Position && checkMove(posY, height, new CPos(0, Velocity.Y, Velocity.Z)))
					return;
			}

			denyMove();
		}

		bool checkMove(CPos pos, int height, CPos velocity)
		{
			var oldPos = Position;
			var oldHeight = Height;

			Height = height;
			Position = pos;

			var intersects = World.CheckCollision(this);

			Position = oldPos;
			Height = oldHeight;

			if (intersects || !World.ActorInWorld(pos, this))
				return false;

			var terrain = World.TerrainAt(pos);
			if (terrain != null && height == 0 && terrain.Type.Speed == 0)
				return false;

			acceptMove(pos, height, terrain);
			Velocity = velocity;

			return true;
		}

		void acceptMove(CPos position, int height, Terrain terrain)
		{
			var old = Position;
			Height = height;
			Position = position;
			TerrainPosition = Position.ToWPos();
			CurrentTerrain = terrain;
			Physics.Position = position;

			Angle = (old - position).FlatAngle;
			World.PhysicsLayer.UpdateSectors(this);
			World.ActorLayer.Update(this);

			foreach (var part in moveParts)
				part.OnMove(old, Velocity);

			if (CurrentAction.Type == ActionType.MOVE)
			{
				CurrentAction.ExtendAction(1);
				return;
			}

			var action = new ActorAction(ActionType.MOVE, true);
			action.ExtendAction(1);
			QueueAction(action);
		}

		void denyMove()
		{
			Physics.Position = Position;
			Velocity = CPos.Zero;

			CurrentAction = ActorAction.Default;

			foreach (var part in stopParts)
				part.OnStop();
		}

		public void CastSpell(Spells.Spell spell)
		{
			if (World.Game.Editor)
				return;

			Effects.Add(new EffectPart(this, spell));
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

			if (Effects.Any(e => e.Active && e.Spell.Type == Spells.EffectType.INVISIBILITY))
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
			localTick++;

			if (!IsAlive)
				return;

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

			if (reloadDelay-- < 0)
				reloadDelay = 0;

			if (WorldPart != null && WorldPart.Hover > 0)
				Height += (int)(Math.Sin(localTick / 32f) * WorldPart.Hover * 0.5f);

			if (Mobility != null)
			{
				if (Mobility.Velocity != CPos.Zero)
					move();

				if (Mobility.CanFly && WorldPart != null)
				{
					if (Height > WorldPart.Height + WorldPart.Hover * 64)
						AccelerateHeight(false);
					else if (Height < WorldPart.Height - WorldPart.Hover * 64)
						AccelerateHeight(true);
				}

				// Make it impossible to be in the ground.
				if (Height < 0)
					Height = 0;
			}

			if (Health != null)
			{
				if (Health.HP <= 0)
					Killed(null);

				foreach (var effect in Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.HEALTH))
					Health.HP += (int)effect.Spell.Value;
			}

			foreach (var part in tickParts)
				part.Tick();

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

		public bool QueueAction(ActorAction action, bool setUpcoming = false)
		{
			if (action.ActionOver)
				return true;

			if (CurrentAction.Skippable || CurrentAction.ActionOver)
			{
				CurrentAction = action;
				return true;
			}

			if (!setUpcoming || upcoming != null)
				return false;

			upcoming = action;

			return true;
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
			if (reloadDelay != 0 || ActiveWeapon == null || !IsAlive)
				return;

			if (World.Game.Editor && IsPlayer || !World.Map.Type.AllowWeapons)
				return;

			if (CurrentAction.Type == ActionType.PREPARE_ATTACK)
				return;

			if (Effects.Any(e => e.Active && e.Spell.Type == Spells.EffectType.STUN))
				return;

			var action = new ActorAction(ActionType.PREPARE_ATTACK, true);
			action.ExtendAction(ActiveWeapon.Type.PreparationDelay);
			if (!QueueAction(action))
				return;

			Angle = (Position - target.Position).FlatAngle;

			ActiveWeapon.OnAttack(target);
		}

		public void AttackWith(Target target, Weapon weapon)
		{
			if (World.Game.Editor && IsPlayer || !World.Map.Type.AllowWeapons)
				return;

			var action = new ActorAction(ActionType.ATTACK, false);
			action.ExtendAction(ActiveWeapon.Type.ShootDuration);
			QueueAction(action);
			
			var cooldownAction = new ActorAction(ActionType.END_ATTACK, false);
			cooldownAction.ExtendAction(ActiveWeapon.Type.CooldownDelay);
			QueueAction(cooldownAction, ActiveWeapon.Type.ShootDuration != 0); // Directly use when shootDuration is 0

			World.Add(weapon);

			foreach (var part in PartManager.GetOrDefault<INoticeAttack>())
				part.OnAttack(target.Position, weapon);

			var reloadModifier = 1f;
			foreach (var effect in Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.COOLDOWN))
				reloadModifier *= effect.Spell.Value;

			reloadDelay = (int)(ActiveWeapon.Type.Reload * reloadModifier);
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

			if (Effects.Any(e => e.Active && e.Spell.Type == Spells.EffectType.SHIELD))
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
