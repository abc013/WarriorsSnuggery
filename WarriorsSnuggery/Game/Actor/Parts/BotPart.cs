using System.Linq;

namespace WarriorsSnuggery.Objects.Parts
{
	public class BotPart : ActorPart
	{
		readonly World world;

		const int searchIntervall = 20;

		bool canMove
		{
			get { return self.Mobility != null; }
			set { }
		}
		bool canAttack
		{
			get { return self.ActiveWeapon != null; }
			set { }
		}
		float distToTarget
		{
			get { return Target.Position.Dist(self.Position); }
			set { }
		}
		float angleToTarget
		{
			get { return Target.Position.Angle(self.Position); }
			set { }
		}

		int inRage;

		public Actor Target;
		float targetFavor;

		public BotPart(Actor self) : base(self)
		{
			world = self.World;
		}

		public override void Tick()
		{
			if (Target == null || !Target.IsAlive || Target.Disposed)
			{
				searchTarget();
			}
			else
			{
				// Look if we have a weapon and are in weapon range
				if (canAttack)
				{
					self.ActiveWeapon.Target = Target.Position;
					int range = self.ActiveWeapon.Type.MaxRange;
					range /= inRage > 40 ? 2 : 1;

					if (distToTarget < range)
					{
						self.Attack(Target);
						inRage++;
						if (canMove)
							self.Accelerate(-angleToTarget);
					}
					else
					{
						// If Actor can move, then move to target; if not, it can't attack and can't move. So it will search a new target.
						if (canMove)
							self.Accelerate(angleToTarget);
						else
							searchTarget();
					}
				}
			}

			if (inRage < -50)
				inRage = -50;
			if (inRage > 50)
				inRage = 50;
		}

		public override void OnDamage(Actor damager, int damage)
		{
			inRage -= damage * 5;

			if (damager == null || damager.Health == null)
				return;

			checkTarget(damager);
		}

		public override void OnKill(Actor killer)
		{
			inRage += self.ActiveWeapon.Type.Damage * 10;
		}

		void searchTarget()
		{
			if (world.Game.LocalTick % searchIntervall != 0)
				return;
			inRage--;

			float range = 10240f;
			if (canAttack)
				range = self.ActiveWeapon.Type.MaxRange * (canMove ? 1.5f : 1f);

			// Find all possible targets in range
			var targets = world.Actors.FindAll(a => a.Team != Actor.NeutralTeam && a.Team != self.Team && a.Position.Dist(self.Position) <= range);

			if (!targets.Any())
				return;

			// Loop through and find the best target
			foreach (var actor in targets)
			{
				if (actor.Health == null || !actor.IsAlive)
					continue;

				checkTarget(actor);
			}
		}

		void checkTarget(Actor actor)
		{
			if (Target == null || !Target.IsAlive)
			{
				Target = actor;
				return;
			}

			var newFavor = 0f;

			// Factor: Health. from 0 to 1
			// If target has less health, then keep attacking it
			newFavor += Target.Health.HPRelativeToMax - actor.Health.HPRelativeToMax;

			// Factor: Distance.
			// If target is closer, then keep attacking it
			newFavor += 1 - self.Position.Dist(actor.Position) / distToTarget;

			// Factor: Player. from 0 to 1
			// If target is player, then keep attacking it
			newFavor += actor.IsPlayer ? 1 : 0;

			if (newFavor > targetFavor)
			{
				Target = actor;
				targetFavor = newFavor;
			}
		}
	}
}
