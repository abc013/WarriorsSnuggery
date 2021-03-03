using System;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Bot
{
	public abstract class BotBehavior
	{
		protected const int SearchIntervall = 20;

		public Target Target;
		protected float TargetFavor;

		protected readonly World World;
		protected readonly Actor Self;

		protected float DistToMapEdge
		{
			get
			{
				int x = World.Map.BottomRightCorner.X;
				if (Self.Position.X > World.Map.Bounds.X * 512 - 256)
					x -= Self.Position.X;
				else
					x += Self.Position.X;

				int y = World.Map.BottomLeftCorner.Y;
				if (Self.Position.Y > World.Map.Bounds.Y * 512 - 256)
					y -= Self.Position.Y;
				else
					y += Self.Position.Y;

				return Math.Min(x, y);
			}
		}

		protected float AngleToMapMid => (Self.Position - World.Map.Center.ToCPos()).FlatAngle;

		protected float DistToTarget => (Target.Position - Self.Position).FlatDist;
		protected float AngleToTarget => (Self.Position - Target.Position).FlatAngle;

		protected bool CanMove => Self.Mobility != null;
		protected bool CanAttack => Self.ActiveWeapon != null;

		protected float AngleToNearActor
		{
			get
			{
				var sectors = World.ActorLayer.GetSectors(Self.Position, 2560);
				foreach (var sector in sectors)
				{
					foreach (var actor in sector.Actors)
					{
						if (actor == Self || actor.Team != Self.Team)
							continue;

						var dist = Self.Position - actor.Position;
						if (dist.SquaredFlatDist < 1024 * 1024)
							return dist.FlatAngle;
					}
				}

				return float.NegativeInfinity;
			}
		}

		protected BotBehavior(World world, Actor self)
		{
			World = world;
			Self = self;
		}

		public abstract void Tick();

		public abstract void OnDamage(Actor damager, int damage);

		public virtual void OnKill(Actor killed)
		{
			if (Target == null)
				return;

			if (killed == Target.Actor)
			{
				if (Target.Actor.FollowupActor == null)
					Target = new Target(killed.Position, killed.Height);
				else
					Target = new Target(Target.Actor.FollowupActor);
			}
		}

		protected virtual bool PerfectTarget()
		{
			if (Target == null || Target.Actor == null)
				return false;

			if (!Target.Actor.IsAlive || Target.Actor.Disposed)
				return false;

			return true;
		}

		protected virtual void SearchTarget()
		{
			if (World.Game.LocalTick % SearchIntervall != 0)
				return;

			var range = Self.RevealsShroudPart == null ? 5120 : Self.RevealsShroudPart.Range * 512;

			// Find all possible targets in range
			var sectors = World.ActorLayer.GetSectors(Self.Position, range);

			// Loop through and find the best target
			foreach (var sector in sectors)
			{
				foreach (var actor in sector.Actors)
				{
					if (actor.Health == null || !actor.IsAlive || actor.Team == Self.Team || actor.Team == Actor.NeutralTeam)
						continue;

					var dist = (actor.Position - Self.Position).SquaredFlatDist;
					if (dist <= range * range)
						CheckTarget(actor);
				}
			}
		}

		protected virtual void CheckTarget(Actor actor)
		{
			if (actor.Team == Self.Team)
				return;

			if (!PerfectTarget())
			{
				Target = new Target(actor);
				return;
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
			}
		}
	}
}
