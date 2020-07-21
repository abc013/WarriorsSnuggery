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
				int x;
				if (Self.Position.X > World.Map.Bounds.X * 512 - 256)
					x = World.Map.BottomRightCorner.X - Self.Position.X;
				else
					x = World.Map.BottomLeftCorner.X + Self.Position.X;

				int y;
				if (Self.Position.Y > World.Map.Bounds.Y * 512 - 256)
					y = World.Map.BottomLeftCorner.Y - Self.Position.Y;
				else
					y = World.Map.TopRightCorner.Y + Self.Position.Y;

				return x > y ? y : x;
			}
		}

		protected float AngleToMapMid
		{
			get
			{
				return (Self.Position - World.Map.Center.ToCPos()).FlatAngle;
			}
		}

		protected float DistToTarget
		{
			get { return (Target.Position - Self.Position).FlatDist; }
			set { }
		}
		protected float AngleToTarget
		{
			get { return (Self.Position - Target.Position).FlatAngle; }
			set { }
		}

		protected bool CanMove
		{
			get { return Self.Mobility != null; }
			set { }
		}
		protected bool CanAttack
		{
			get { return Self.ActiveWeapon != null; }
			set { }
		}

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
			set { }
		}

		protected BotBehavior(World world, Actor self)
		{
			World = world;
			Self = self;
		}

		public abstract void Tick();

		public abstract void OnDamage(Actor damager, int damage);

		public abstract void OnKill(Actor killer);

		protected bool PerfectTarget()
		{
			return Target != null && Target.Actor != null && Target.Actor.IsAlive && !Target.Actor.Disposed;
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

			if (Target == null || Target.Actor == null || !Target.Actor.IsAlive || !Target.Actor.Disposed)
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
