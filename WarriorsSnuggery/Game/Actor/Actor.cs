using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Effects;
using WarriorsSnuggery.Objects.Parts;
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
				BotPart = new BotPart(this);
				Parts.Add(BotPart);
			}
		}

		public void Accelerate(CPos target, int customAcceleration = 0)
		{
			Accelerate((Position - target).FlatAngle, customAcceleration);
		}

		public void Accelerate(float angle, int customAcceleration = 0)
		{
			if (!IsAlive || Mobility == null)
				return;

			var acceleration = Mobility.OnAccelerate(angle, customAcceleration);

			Parts.ForEach(p => p.OnAccelerate(angle, acceleration));
		}

		void move()
		{
			if (!IsAlive || Mobility == null || Velocity == CPos.Zero)
				return;

			var currentTerrain = World.TerrainAt(Position);
			if (currentTerrain == null) return;

			var speedModifier = Height == 0 ? currentTerrain.Type.Speed : 1f;
			if (speedModifier.Equals(0)) return;

			var movement = new MPos((int)Math.Round(Velocity.X * speedModifier), (int)Math.Round(Velocity.Y * speedModifier));
			if (movement == MPos.Zero) return;

			var pos = new CPos(Position.X + movement.X, Position.Y + movement.Y, Position.Z);
			var oldpos = Position;

			var ignoreTypes = new[] { typeof(Weapon), typeof(BeamWeapon), typeof(BulletWeapon), typeof(RocketWeapon) };
			Position = pos;
			var intersects = World.CheckCollision(this, false, ignoreTypes);
			Position = oldpos;
			var terrain = World.TerrainAt(pos);

			if (World.IsInWorld(pos) && !intersects && !(terrain == null || (terrain.Type.Speed.Equals(0) && Height == 0)))
			{
				acceptMove(pos);
				return;
			}

			var posX = new CPos(Position.X + movement.X, Position.Y, Position.Z);

			Position = posX;
			intersects = World.CheckCollision(this, false, ignoreTypes);
			Position = oldpos;
			terrain = World.TerrainAt(posX);

			if (World.IsInWorld(posX) && !intersects && !(terrain == null || (terrain.Type.Speed.Equals(0) && Height == 0)))
			{
				acceptMove(posX);
				Velocity = new CPos(Velocity.X, 0, 0);
				return;
			}

			var posY = new CPos(Position.X, Position.Y + movement.Y, Position.Z);

			Position = posY;
			intersects = World.CheckCollision(this, false, ignoreTypes);
			Position = oldpos;
			terrain = World.TerrainAt(posY);

			if (World.IsInWorld(posY) && !intersects && !(terrain == null || (terrain.Type.Speed.Equals(0) && Height == 0)))
			{
				acceptMove(posY);
				Velocity = new CPos(0, Velocity.Y, 0);
				return;
			}

			denyMove();
		}

		void acceptMove(CPos position)
		{
			var old = Position;
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

		public override void CheckVisibility()
		{
			// TODO solve detection scale
			visible = VisibilitySolver.IsVisible(GraphicPosition, new MPos(1024, 1024));
		}

		public override void Render()
		{
			if (visible)
			{
				base.Render();

				Parts.ForEach(p => p.Render());
			}
		}

		public void RenderDebug()
		{
			Graphics.ColorManager.DrawDot(Position, Color.Blue);
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
			{
				Height += (int)(Math.Sin(localTick / 32f) * WorldPart.Hover * 0.5f);
			}

			if (Mobility != null)
			{
				if (Mobility.Velocity != CPos.Zero)
					move();
			}

			if (Health != null && Health.HP <= 0)
			{
				if (Health.HP <= 0)
				{
					Killed(null);
				}
				foreach (var effect in Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.HEALTH))
				{
					Health.HP += (int)effect.Spell.Value;
				}
			}

			Parts.ForEach(p => p.Tick());

			Effects.ForEach(e => e.Tick());
			Effects.RemoveAll(e => !e.Active);
		}

		public void Attack(CPos target)
		{
			if (reloadDelay != 0 || ActiveWeapon == null || !IsAlive)
				return;

			if (World.Game.Type == GameType.EDITOR || World.Game.Editor && IsPlayer || !World.Map.Type.AllowWeapons)
				return;

			if ((Position - target).FlatDist < ActiveWeapon.Type.MinRange)
				return;

			Angle = (Position - target).FlatAngle;

			var weapon = ActiveWeapon.OnAttack(target);

			Parts.ForEach(p => p.OnAttack(target, weapon));

			var reloadModifier = 1f;
			foreach (var effect in Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.COOLDOWN))
			{
				reloadModifier *= effect.Spell.Value;
			}

			reloadDelay = (int)(ActiveWeapon.Type.Reload * reloadModifier);
			CurrentAction = ActorAction.ATTACKING;
		}

		public void Attack(Actor target)
		{
			Attack(target.Position);
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
			//parts.Clear(); TODO?
		}
	}
}
