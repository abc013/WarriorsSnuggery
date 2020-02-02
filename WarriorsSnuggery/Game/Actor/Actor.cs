using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Effects;
using WarriorsSnuggery.Objects.Parts;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public enum ActorAction
	{
		IDLING,
		ATTACKING,
		MOVING,
		ALL
	}

	public sealed class Actor : PhysicsObject
	{
		public readonly World World;

		public readonly bool IsPlayer;
		public readonly bool IsBot;
		public readonly uint ObjectID;

		public bool IsAlive = true;

		public const byte PlayerTeam = 2;
		public const byte NeutralTeam = 0;

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

		int localTick;
		int reloadDelay;

		public bool WeaponReloading
		{
			get { return reloadDelay > 0; }
		}

		bool visible;

		CPos Velocity
		{
			get
			{
				return Mobility == null ? CPos.Zero : Mobility.Velocity;
			}
			set
			{
				if (Mobility != null)
					Mobility.Velocity = value;
			}
		}
		public ActorAction CurrentAction;

		public Actor(World world, ActorType type, CPos position, byte team, bool isBot, bool isPlayer = false) : base(position, null, type.Physics == null ? null : new SimplePhysics(position, 0, type.Physics.Shape, type.Physics.Size.X, type.Physics.Size.Y, type.Physics.Size.Z))
		{
			World = world;
			Type = type;
			Team = team;
			IsPlayer = isPlayer;
			CurrentAction = ActorAction.IDLING;
			IsBot = isBot;

			ObjectID = world.Game.NextObjectID;

			// Parts
			foreach (var partinfo in type.PartInfos)
			{
				Parts.Add(partinfo.Create(this));
			}

			Mobility = (MobilityPart)Parts.Find(p => p is MobilityPart);
			Health = (HealthPart)Parts.Find(p => p is HealthPart);

			RevealsShroudPart = (RevealsShroudPart)Parts.Find(p => p is RevealsShroudPart);

			ActiveWeapon = (WeaponPart)Parts.Find(p => p is WeaponPart);

			WorldPart = (WorldPart)Parts.Find(p => p is WorldPart);
			if (WorldPart != null)
				Height = WorldPart.Height;

			if (isPlayer)
				Parts.Add(new PlayerPart(this));

			if (isBot)
			{
				var behavior = WorldPart == null ? Bot.BotBehaviorType.TYPICAL : WorldPart.BotBehavior;
				BotPart = new BotPart(this, behavior);
				Parts.Add(BotPart);
			}
		}

		public void Accelerate(CPos acceleration, bool forced = false)
		{
			if (Mobility == null)
				return;

			if (!forced && !canMove())
				return;

			Mobility.OnAccelerate(acceleration);
			Parts.ForEach(p => p.OnAccelerate(acceleration));
		}

		public void Accelerate(float angle, bool forced = false, int customAcceleration = 0)
		{
			if (Mobility == null)
				return;

			if (!forced && !canMove())
				return;

			var acceleration = Mobility.OnAccelerate(angle, customAcceleration);
			Parts.ForEach(p => p.OnAccelerate(angle, acceleration));
		}

		public void AccelerateHeight(bool up, bool forced = false, int customAcceleration = 0)
		{
			if (Mobility == null)
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

			if (Effects.Any(e => e.Active && e.Spell.Type == Spells.EffectType.STUN))
				return false;

			return true;
		}

		void move()
		{
			if (!IsAlive || Mobility == null || Velocity == CPos.Zero)
				return;

			var currentTerrain = World.TerrainAt(Position);
			if (currentTerrain == null) return;

			var speedModifier = Height == 0 ? currentTerrain.Type.Speed : 1f;
			if (speedModifier.Equals(0)) return;

			var movement = new CPos((int)Math.Round(Velocity.X * speedModifier), (int)Math.Round(Velocity.Y * speedModifier), (int)Math.Round(Velocity.Z * speedModifier));
			if (movement == CPos.Zero) return;

			var oldpos = Position;
			var oldHeight = Height;

			var pos = new CPos(Position.X + movement.X, Position.Y + movement.Y, Position.Z);
			var height = Height + movement.Z;

			Position = pos;
			Height = height;
			var intersects = World.CheckCollision(this, false);
			Position = oldpos;
			Height = oldHeight;
			var terrain = World.TerrainAt(pos);

			if (World.IsInWorld(pos) && !intersects && !(terrain == null || (terrain.Type.Speed.Equals(0) && Height == 0)))
			{
				acceptMove(pos, height);
				return;
			}

			var posX = new CPos(Position.X + movement.X, Position.Y, Position.Z);

			Position = posX;
			Height = height;
			intersects = World.CheckCollision(this, false);
			Position = oldpos;
			Height = oldHeight;
			terrain = World.TerrainAt(posX);

			if (World.IsInWorld(posX) && !intersects && !(terrain == null || (terrain.Type.Speed.Equals(0) && Height == 0)))
			{
				acceptMove(posX, height);
				Velocity = new CPos(Velocity.X, 0, Velocity.Z);
				return;
			}

			var posY = new CPos(Position.X, Position.Y + movement.Y, Position.Z);

			Position = posY;
			intersects = World.CheckCollision(this, false);
			Position = oldpos;
			Height = oldHeight;
			terrain = World.TerrainAt(posY);

			if (World.IsInWorld(posY) && !intersects && !(terrain == null || (terrain.Type.Speed.Equals(0) && Height == 0)))
			{
				acceptMove(posY, height);
				Velocity = new CPos(0, Velocity.Y, Velocity.Z);
				return;
			}

			denyMove();
		}

		void acceptMove(CPos position, int height)
		{
			var old = Position;
			Height = height;
			Position = position;
			if (Physics != null)
				Physics.Position = position;

			CheckVisibility();
			CurrentAction = ActorAction.MOVING;
			Angle = (old - position).FlatAngle;
			World.PhysicsLayer.UpdateSectors(this);

			Parts.ForEach(p => p.OnMove(old, Velocity));
		}

		void denyMove()
		{
			Physics.Position = Position;
			Velocity = CPos.Zero;

			Parts.ForEach(p => p.OnStop());
		}

		public void CastSpell(Spells.Spell spell)
		{
			Effects.Add(new EffectPart(this, spell));
		}

		public override void CheckVisibility()
		{
			// TODO solve detection scale
			visible = VisibilitySolver.IsVisible(GraphicPosition, new MPos(1024, 1024));
		}

		public override void Render()
		{
			if (Effects.Any(e => e.Active && e.Spell.Type == Spells.EffectType.INVISIBILITY))
				return;

			if (visible)
			{
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
			CurrentAction = ActorAction.IDLING;

			if (!IsAlive)
				return;

			reloadDelay--;
			if (reloadDelay < 0) reloadDelay = 0;

			if (WorldPart != null && WorldPart.Hover > 0)
				Height += (int)(Math.Sin(localTick / 32f) * WorldPart.Hover * 0.5f);

			if (Mobility != null)
			{
				if (Mobility.Velocity != CPos.Zero)
					move();

				if (WorldPart != null && Mobility.CanFly)
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

			if (Health != null && Health.HP <= 0)
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

		public void Attack(Actor target)
		{
			Attack(new Target(target));
		}

		public void Attack(CPos target, int height)
		{
			Attack(new Target(target, height));
		}

		public void Attack(Target target)
		{
			if (reloadDelay != 0 || ActiveWeapon == null || !IsAlive)
				return;

			if (World.Game.Type == GameType.EDITOR || World.Game.Editor && IsPlayer || !World.Map.Type.AllowWeapons)
				return;

			if (Effects.Any(e => e.Active && e.Spell.Type == Spells.EffectType.STUN))
				return;

			Angle = (Position - target.Position).FlatAngle;

			ActiveWeapon.OnAttack(target);
		}

		public void AttackWith(Target target, Weapon weapon)
		{
			Parts.ForEach(p => p.OnAttack(target.Position, weapon));

			var reloadModifier = 1f;
			foreach (var effect in Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.COOLDOWN))
				reloadModifier *= effect.Spell.Value;

			reloadDelay = (int)(ActiveWeapon.Type.Reload * reloadModifier);
			CurrentAction = ActorAction.ATTACKING;
		}

		public void Kill(Actor killed)
		{
			Parts.ForEach(p => p.OnKill(killed));
		}

		public void Damage(Actor attacker, int damage)
		{
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
		}
	}
}
