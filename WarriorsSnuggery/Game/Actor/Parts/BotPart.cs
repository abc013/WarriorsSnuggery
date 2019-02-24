using System;
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
			get { return target.Position.GetDistToXY(self.Position); }
			set { }
		}
		float angleToTarget
		{
			get { return target.Position.GetAngleToXY(self.Position); }
			set { }
		}

		int inRage;

		Actor target;
		float targetfavor;

		public BotPart(Actor self) : base(self)
		{
			world = self.World;
		}

		public override void Tick()
		{
			if (target == null || !target.IsAlive)
			{
				searchTarget();
			}
			else
			{
				// Look if we have a weapon and are in weapon range
				if (canAttack)
				{
					int range = self.ActiveWeapon.Type.MaxRange;
					range /= inRage > 40 ? 2 : 1;

					if (distToTarget < range)
					{
						self.Attack(target);
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

			if (damager.Health == null)
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
			var targets = world.Actors.FindAll(a => a.Team != 0 && a.Team != self.Team && a.Position.GetDistToXY(self.Position) <= range);

			if (!targets.Any())
				return;
			
			// Loop through and find the best target
			foreach(var actor in targets)
			{
				if (actor.Health == null || !actor.IsAlive)
					continue;
				
				checkTarget(actor);
			}
		}

		void checkTarget(Actor actor)
		{
			// We determine why we should use this actor as a target. Currently, the only factors are health and distance.
			// TODO: to add: Damage, Speed; use a float to determine whether this is better as the last target or not.
			if (target == null || !target.IsAlive)
			{
				target = actor;
				return;
			}

			var newFavor = 0f;

			newFavor += (actor.Health.HP - target.Health.HP) * 256;

			newFavor += distToTarget - target.Position.GetDistToXY(actor.Position);

			if (targetfavor < newFavor)
			{
				target = actor;
				targetfavor = newFavor;
			}
		}
	}
}
