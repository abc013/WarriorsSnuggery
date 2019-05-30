﻿/*
 * User: Andreas
 * Date: 22.10.2017
 * 
 */
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Parts;

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

		public const ushort PlayerTeam = 2;
		public const ushort NeutralTeam = 0;

		public ushort Team;
		public float Angle;
		
		readonly List<ActorPart> parts = new List<ActorPart>();

		public readonly MobilityPart Mobility;
		public readonly HealthPart Health;

		public WeaponPart ActiveWeapon;

		public readonly WorldPart WorldPart;

		public readonly ActorType Type;
		
		int LocalTick;
		int ReloadDelay;

		CPos Velocity {
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

		public Actor(World world, ActorType type, CPos position, ushort team, bool isBot, bool isPlayer = false) : base(position, null, type.Physics == null ? null : new Physics(position, 0, type.Physics.Shape, type.Physics.Size.X, type.Physics.Size.Y, type.Physics.Size.Z))
		{
			World = world;
			Type = type;
			Team = team;
			IsPlayer = isPlayer;
			CurrentAction = ActorAction.IDLING;
			Height = type.Height;
			IsBot = isBot;

			ObjectID = world.Game.NextObjectID;

			// Parts
			foreach (var partinfo in type.PartInfos)
			{
				parts.Add(partinfo.Create(this));
			}

			Mobility = (MobilityPart) parts.Find(p => p is MobilityPart);
			Health = (HealthPart) parts.Find(p => p is HealthPart);

			ActiveWeapon = (WeaponPart) parts.Find(p => p is WeaponPart);

			WorldPart = (WorldPart) parts.Find(p => p is WorldPart);

			if (Settings.DeveloperMode)
				parts.Add(new DebugPart(this));

			if (isPlayer)
			{
				foreach (var node in TechTreeLoader.TechTree)
				{
					if (node.Unlocked || world.Game.Statistics.UnlockedNodes.ContainsKey(node.InnerName) && world.Game.Statistics.UnlockedNodes[node.InnerName])
						parts.Add(new EffectPart(this, node.Effect));
				}

				parts.Add(new PlayerPart(this));
			}

			if (isBot)
				parts.Add(new BotPart(this));
		}

		public void Accelerate(CPos target, int customAcceleration = 0)
		{
			Accelerate(target.GetAngleToXY(Position), customAcceleration);
		}

		public void Accelerate(float angle, int customAcceleration = 0)
		{
			if (!IsAlive || Mobility == null)
				return;

			var acceleration = Mobility.OnAccelerate(angle, customAcceleration);

			parts.ForEach(p => p.OnAccelerate(angle, acceleration));
		}

		void move()
		{
			if (!IsAlive || Mobility == null || Velocity == CPos.Zero) // PERF: increase accuracy by timestamp*speed / 2
				return;

			var currentTerrain = World.TerrainAt(Position);
			if (currentTerrain == null) return;

			var speedModifier = Height == 0 ? currentTerrain.Type.SpeedModifier : 1f;
			if (speedModifier.Equals(0)) return;

			var movement = new MPos((int) Math.Round(Velocity.X * speedModifier),(int) Math.Round(Velocity.Y * speedModifier));
			if (movement == MPos.Zero) return;

			var pos = new CPos(Position.X + movement.X, Position.Y + movement.Y, Position.Z);
			var oldpos = Position;

			var ignoreTypes = new[] { typeof(Weapon), typeof(BeamWeapon), typeof(BulletWeapon), typeof(RocketWeapon) };
			Position = pos;
			var intersects = World.CheckCollision(this, false, ignoreTypes);
			Position = oldpos;
			var terrain = World.TerrainAt(pos);

			if(World.IsInWorld(pos) && !intersects && !(terrain == null || (terrain.Type.SpeedModifier.Equals(0) && Height == 0)))
			{
				acceptMove(pos);
				return;
			}

			var posX = new CPos(Position.X + movement.X, Position.Y, Position.Z);

			Position = posX;
			intersects = World.CheckCollision(this, false, ignoreTypes);
			Position = oldpos;
			terrain = World.TerrainAt(posX);

			if(World.IsInWorld(posX) && !intersects && !(terrain == null || (terrain.Type.SpeedModifier.Equals(0) && Height == 0)))
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

			if(World.IsInWorld(posY) && !intersects && !(terrain == null || (terrain.Type.SpeedModifier.Equals(0) && Height == 0)))
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
			Angle = Position.GetAngleToXY(old);
			World.PhysicsLayer.UpdateSectors(this);

			parts.ForEach(p => p.OnMove(old, Velocity));
		}

		void denyMove()
		{
			Physics.Position = Position;
			Velocity = CPos.Zero;

			parts.ForEach(p => p.OnStop());
		}

		public override void Render()
		{
			base.Render();
			
			parts.ForEach(p => p.Render());
		}

		public override void Tick()
		{
			base.Tick();
			LocalTick++;
			CurrentAction = ActorAction.IDLING;

			if(!IsAlive)
				return;
			
			ReloadDelay--;
			if (ReloadDelay < 0) ReloadDelay = 0;

			if (Height > 128)
				Height += (int) (Math.Sin(LocalTick/32f) * 4);

			if (Mobility != null)
			{
				Mobility.Tick();
				if (Mobility.Velocity != CPos.Zero)
					move();
			}

			if (Health != null && Health.HP <= 0)
			{
				Killed(null);
			}

			parts.ForEach(p => p.Tick());
		}

		public void Attack(CPos target)
		{
			if (ReloadDelay != 0 || ActiveWeapon == null || !IsAlive || !World.IsInWorld(target))
				return;

			if (World.Game.Type == GameType.EDITOR || World.Game.Editor && IsPlayer)
				return;

			if (Position.GetDistToXY(target) < ActiveWeapon.Type.MinRange)
				return;

			Angle = target.GetAngleToXY(Position);

			var weapon = ActiveWeapon.OnAttack(target);

			parts.ForEach(p => p.OnAttack(target, weapon));

			ReloadDelay = ActiveWeapon.Type.Reload;
			CurrentAction = ActorAction.ATTACKING;
		}

		public void Attack(Actor target)
		{
			Attack(target.Position);
		}

		public void Kill(Actor killed)
		{
			parts.ForEach(p => p.OnKill(killed));
		}

		public void Damage(Actor attacker, int damage)
		{
			if (Health == null || Health.HP <= 0)
				return;
			
			Health.HP -= damage;

			parts.ForEach(p => p.OnDamage(attacker, damage));

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

			parts.ForEach(p => p.OnKilled(killer));

			Dispose();
		}

		public override void Dispose()
		{
			base.Dispose();

			parts.ForEach(p => p.OnDispose());
			//parts.Clear(); TODO?
		}
	}
}
