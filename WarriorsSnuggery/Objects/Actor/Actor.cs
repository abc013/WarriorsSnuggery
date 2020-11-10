using OpenTK.Graphics.ES20;
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

		public byte Team;
		public float Angle;

		public readonly List<ActorPart> Parts = new List<ActorPart>();
		public readonly List<EffectPart> Effects = new List<EffectPart>();

		public readonly MobilityPart Mobility;
		public readonly HealthPart Health;

		public readonly RevealsShroudPart RevealsShroudPart;

		public WeaponPart ActiveWeapon;

		public readonly WorldPart WorldPart;

		public readonly BotPart BotPart;

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

		public Actor(World world, ActorType type, CPos position, byte team, bool isBot, bool isPlayer, uint id) : base(position, null, getPhysics(position, type))
		{
			World = world;
			Type = type;
			Team = team;
			IsPlayer = isPlayer;
			IsBot = isBot;

			ID = id;
			TerrainPosition = position.ToWPos();
			CurrentTerrain = world.TerrainAt(TerrainPosition);

			// Parts
			foreach (var partinfo in type.PartInfos)
				Parts.Add(partinfo.Create(this));

			Mobility = (MobilityPart)Parts.Find(p => p is MobilityPart);
			Health = (HealthPart)Parts.Find(p => p is HealthPart);

			RevealsShroudPart = (RevealsShroudPart)Parts.Find(p => p is RevealsShroudPart);

			ActiveWeapon = (WeaponPart)Parts.Find(p => p is WeaponPart);

			WorldPart = (WorldPart)Parts.Find(p => p is WorldPart);
			if (WorldPart != null)
				Height = WorldPart.Height;

			IsPlayerSwitch = Parts.Any(p => p is PlayerSwitchPart);

			if (isPlayer)
				Parts.Add(new PlayerPart(this));

			if (isBot)
			{
				var behavior = WorldPart == null ? Bot.BotBehaviorType.TYPICAL : WorldPart.BotBehavior;
				BotPart = new BotPart(this, behavior);
				Parts.Add(BotPart);
			}
		}

		public Actor(World world, ActorInit init, uint overrideID) : base(init.Position, null, getPhysics(init.Position, init.Type))
		{
			World = world;
			Type = init.Type;

			Height = init.Height;

			Team = init.Convert("Team", (byte)0);
			IsPlayer = init.Nodes.Any(n => n.Key == "PlayerPart");
			IsBot = init.Nodes.Any(n => n.Key == "BotPart");

			ID = overrideID;
			this.init = init;
			TerrainPosition = init.Position.ToWPos();
			CurrentTerrain = world.TerrainAt(TerrainPosition);

			// Parts
			foreach (var partinfo in Type.PartInfos)
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
			var list = new List<string>
			{
				"Type=" + ActorCreator.GetName(Type),
				"Position=" + Position,
				"Height=" + Height,
				"Team=" + Team
			};

			foreach (var part in Parts)
				list.AddRange(part.OnSave().GetSave());

			foreach (var part in Effects)
				list.AddRange(part.Save());

			return list;
		}

		public void Accelerate(CPos acceleration, bool forced = false)
		{
			if (Mobility == null || World.Game.Editor)
				return;

			if (!forced && !canMove())
				return;

			Mobility.OnAccelerate(acceleration);
			Parts.ForEach(p => p.OnAccelerate(acceleration));
		}

		public void Accelerate(float angle, bool forced = false, int customAcceleration = 0)
		{
			if (Mobility == null || World.Game.Editor)
				return;

			if (!forced && !canMove())
				return;

			var acceleration = Mobility.OnAccelerate(angle, customAcceleration);
			Parts.ForEach(p => p.OnAccelerate(angle, acceleration));
		}

		public void AccelerateHeight(bool up, bool forced = false, int customAcceleration = 0)
		{
			if (Mobility == null || World.Game.Editor)
				return;

			if (!forced && (!Mobility.CanFly || !canMove()))
				return;

			var acceleration = Mobility.OnAccelerateHeight(up, customAcceleration);
			Parts.ForEach(p => p.OnAccelerate(new CPos(0, 0, acceleration)));
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

			var oldpos = Position;
			var oldHeight = Height;

			var pos = new CPos(Position.X + movement.X, Position.Y + movement.Y, Position.Z);
			var height = Height + movement.Z;

			Position = pos;
			Height = height;
			var intersects = World.CheckCollision(this);
			Position = oldpos;
			Height = oldHeight;
			var terrain = World.TerrainAt(pos);

			if (World.ActorInWorld(pos, this) && !intersects && !(terrain == null || (terrain.Type.Speed.Equals(0) && Height == 0)))
			{
				acceptMove(pos, height, terrain);
				return;
			}

			var posX = new CPos(Position.X + movement.X, Position.Y, Position.Z);

			Position = posX;
			Height = height;
			intersects = World.CheckCollision(this);
			Position = oldpos;
			Height = oldHeight;
			terrain = World.TerrainAt(posX);

			if (World.ActorInWorld(posX, this) && !intersects && !(terrain == null || (terrain.Type.Speed.Equals(0) && Height == 0)))
			{
				acceptMove(posX, height, terrain);
				Velocity = new CPos(Velocity.X, 0, Velocity.Z);
				return;
			}

			var posY = new CPos(Position.X, Position.Y + movement.Y, Position.Z);

			Position = posY;
			intersects = World.CheckCollision(this);
			Position = oldpos;
			Height = oldHeight;
			terrain = World.TerrainAt(posY);

			if (World.ActorInWorld(posY, this) && !intersects && !(terrain == null || (terrain.Type.Speed.Equals(0) && Height == 0)))
			{
				acceptMove(posY, height, terrain);
				Velocity = new CPos(0, Velocity.Y, Velocity.Z);
				return;
			}

			denyMove();
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

			Parts.ForEach(p => p.OnMove(old, Velocity));

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

			Parts.ForEach(p => p.OnStop());
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
			if (visible)
			{
				if (Effects.Any(e => e.Active && e.Spell.Type == Spells.EffectType.INVISIBILITY))
					return;

				base.Render();

				Parts.ForEach(p => p.Render());
			}
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

			reloadDelay--;
			if (reloadDelay < 0) reloadDelay = 0;

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

			Parts.ForEach(p => p.Tick());

			Effects.ForEach(e => e.Tick());
			Effects.RemoveAll(e => !e.Active);
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

			Parts.ForEach(p => p.OnAttack(target.Position, weapon));

			var reloadModifier = 1f;
			foreach (var effect in Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.COOLDOWN))
				reloadModifier *= effect.Spell.Value;

			reloadDelay = (int)(ActiveWeapon.Type.Reload * reloadModifier);
		}

		public void Kill(Actor killed)
		{
			Parts.ForEach(p => p.OnKill(killed));
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

			Parts.ForEach(p => p.OnDamage(attacker, damage));

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

			Parts.ForEach(p => p.OnKilled(killer));

			Dispose();
		}

		public override void Dispose()
		{
			base.Dispose();

			Parts.ForEach(p => p.OnDispose());
			World.ActorLayer.Remove(this);
		}
	}
}
