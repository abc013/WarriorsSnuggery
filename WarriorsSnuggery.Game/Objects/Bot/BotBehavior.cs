using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Objects.Weapons.Projectiles;

namespace WarriorsSnuggery.Objects.Bot
{
	public abstract class BotBehavior
	{
		protected const int SearchIntervall = 20;

		public Target Target
		{
			get => target;
			set
			{
				target = value;
				if (IsLeader)
					Patrol?.SetNewTarget(value);

				if (target.Type == TargetType.POSITION)
					TargetFavor = 0;

				// Calculate path
				if (CanMove)
				{
					Waypoints.Clear();

					var path = Self.World.PathfinderLayer.CalculatePath(Self.TerrainPosition, target.Position.ToMPos(), Self.Mobility.CanFly);

					foreach (var waypoint in path)
						Waypoints.Enqueue(waypoint.ToCPos());
				}
			}
		}
		Target target;
		protected float TargetFavor;

		protected virtual bool PerfectTarget
		{
			get
			{
				if (Target == null || Target.Actor == null)
					return false;

				if (!Target.Actor.IsAlive || Target.Actor.Disposed)
					return false;

				return true;
			}
		}

		protected readonly Queue<CPos> Waypoints = new Queue<CPos>();

		public Patrol Patrol;
		protected bool IsLeader => Patrol == null || Patrol.Leader == Self;

		protected readonly World World;
		protected readonly Actor Self;

		protected float DistToMapEdge
		{
			get
			{
				var bottomRightCorner = World.Map.BottomRightCorner;

				var x = Self.Position.X;
				if (x > bottomRightCorner.X / 2)
					x = bottomRightCorner.X - x;

				var y = Self.Position.Y;
				if (y > bottomRightCorner.Y / 2)
					y -= bottomRightCorner.Y - y;

				return Math.Min(x, y);
			}
		}

		protected float AngleToMapMid => (Self.Position - World.Map.Center.ToCPos()).FlatAngle;

		protected float DistToTarget => (Target.Position - Self.Position).FlatDist;
		protected float AngleToTarget => (Self.Position - Target.Position).FlatAngle;

		protected bool CanMove => Self.Mobility != null;
		protected bool CanAttack => Self.Weapon != null;

		protected BotBehavior(World world, Actor self)
		{
			World = world;
			Self = self;
		}

		public abstract void Tick();

		public virtual void DefaultTickBehavior()
		{
			const int maxDistToTarget = 256;

			SearchTarget();
			if (CanMove && Target != null && DistToTarget > maxDistToTarget)
			{
				if (Waypoints != null && Waypoints.Count > 0)
				{
					var nearest = Waypoints.Peek();

					var diff = Self.Position - nearest;

					Self.AccelerateSelf(diff.FlatAngle);

					if (diff.SquaredFlatDist <= maxDistToTarget * maxDistToTarget)
						Waypoints.Dequeue();
				}
				else
					Self.AccelerateSelf(AngleToTarget);
			}
		}

		public virtual void OnDamage(Actor damager, int damage)
		{
			if (damager == null || damager.Health == null)
				return;

			if (!PerfectTarget)
			{
				var target = new Target(damager.Position, damager.Height);
				if (IsLeader)
					Target = target;
				else
					Patrol.NotifyNewTarget(target);
			}
		}

		public virtual void OnKill(Actor killed)
		{
			if (Target.Type == TargetType.POSITION)
				return;

			if (killed == Target.Actor)
			{
				if (Target.Actor.FollowupActor == null)
					Target = new Target(killed.Position, killed.Height);
				else
					Target = new Target(Target.Actor.FollowupActor);
			}
		}

		protected virtual void SearchTarget()
		{
			if (World.Game.LocalTick % SearchIntervall != 0)
				return;

			var range = Self.RevealsShroud == null ? 5120 : Self.RevealsShroud.Range * 512;

			// Find all possible targets in range
			var sectors = World.ActorLayer.GetSectors(Self.Position, range);

			// Loop through and find the best target
			foreach (var sector in sectors)
			{
				foreach (var actor in sector.Actors)
				{
					if (actor.Health == null || !actor.IsAlive || actor.Team == Self.Team || actor.Team == Actor.NeutralTeam)
						continue;

					if (!World.IsVisibleTo(Self, actor))
						continue;

					if ((actor.Position - Self.Position).SquaredFlatDist > range * range)
						continue;

					if (IsLeader)
					{
						if (CheckTarget(actor))
							return;
					}
					else
					{
						Patrol.NotifyNewTarget(new Target(actor));
					}
				}
			}
		}

		public virtual void CheckTarget(Target target)
		{
			if (!IsLeader)
				return;

			if (target.Actor != null)
			{
				CheckTarget(target.Actor);
				return;
			}

			if (Target == null)
				Target = target;
		}

		protected virtual bool CheckTarget(Actor actor)
		{
			if (actor.Team == Self.Team)
				return false;

			if (!PerfectTarget)
			{
				Target = new Target(actor);

				return true;
			}

			var newFavor = 0f;

			// Factor: Health. from 0 to 1
			// If target has less health, then keep attacking it
			newFavor += Target.Actor.Health.RelativeHP - actor.Health.RelativeHP;

			// Factor: Distance.
			// If target is closer, then keep attacking it
			newFavor += 1 - (Self.Position - actor.Position).FlatDist / DistToTarget;

			// Factor: Player. from 0 to 1
			// If target is player, then keep attacking it
			newFavor += actor.IsPlayer ? 1 : 0;

			if (newFavor > TargetFavor)
			{
				Target = new Target(actor);
				TargetFavor = newFavor;

				return true;
			}

			return false;
		}

		protected Actor GetNeighborActor(bool sameTeam = true, int range = 1536)
		{
			var sectors = World.ActorLayer.GetSectors(Self.Position, range);
			foreach (var sector in sectors)
			{
				foreach (var actor in sector.Actors)
				{
					if (actor == Self || (sameTeam && actor.Team != Self.Team))
						continue;

					var dist = Self.Position - actor.Position;
					if (dist.SquaredFlatDist < range * range)
						return actor;
				}
			}

			return null;
		}

		protected void PredictiveAttack(Target target)
		{
			if (target.Actor == null || target.Actor.Mobility == null || target.Actor.Mobility.Velocity == CPos.Zero)
			{
				Self.PrepareAttack(target);
				return;
			}

			var projectileType = Self.Weapon.Type.Projectile;
			if (projectileType is BeamProjectile || projectileType is InstantHitProjectile)
			{
				Self.PrepareAttack(target);
				return;
			}

			var delta = target.Position - Self.Position;
			var deltaMagnitude = delta.FlatDist;
			var velTarget = target.Actor.Mobility.Velocity;
			var velBullet = projectileType is BulletProjectile projectile ? projectile.MaxSpeed : ((MagicProjectile)projectileType).Speed;

			// See http://danikgames.com/blog/how-to-intersect-a-moving-target-in-2d/ for more information
			// uj, ui: vectors for target velocity in projected space
			// vj, vi: vectors for bullet direction in projected space

			// Find the vector AB, normalize
			var ABx = delta.X / deltaMagnitude;
			var ABy = delta.Y / deltaMagnitude;

			// Project velTarget onto AB
			var uDotAB = ABx * velTarget.X + ABy * velTarget.Y;
			var ujx = uDotAB * ABx;
			var ujy = uDotAB * ABy;

			// Subtract uj from velTarget to get ui
			var uix = velTarget.X - ujx;
			var uiy = velTarget.Y - ujy;

			// Set vi to ui (for clarity)
			var vix = uix;
			var viy = uiy;

			if (velTarget.X < vix && velTarget.Y < viy)
				return;

			// Calculate the magnitude of vj
			var viMag = MathF.Sqrt(vix * vix + viy * viy);
			var vjMag = MathF.Sqrt(velBullet * velBullet - viMag * viMag);

			// Get vj by multiplying it's magnitude with the unit vector AB
			var vjx = ABx * vjMag;
			var vjy = ABy * vjMag;

			// Add vj and vi to get direction
			var direction = new CPos((int)(vjx + vix), (int)(vjy + viy), 0);

			var t = Math.Abs(delta.X / (float)(velTarget.X - direction.X));

			var newTarget = new Target(new CPos(Self.Position.X + (int)(direction.X * t), Self.Position.Y + (int)(direction.Y * t), 0), target.Height);

			Self.PrepareAttack(newTarget);
		}
	}
}
