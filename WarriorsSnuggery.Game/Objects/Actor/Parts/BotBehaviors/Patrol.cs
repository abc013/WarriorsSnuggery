using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	public class Patrol
	{
		readonly List<Actor> group = new List<Actor>();
		public Actor Leader;

		public Patrol(List<Actor> actors, Actor leader = null)
		{
			group = actors.ToList();
			group.RemoveAll(a => a.Bot == null);

			Leader = leader;
			if (Leader == null)
				newLeader();
		}

		public void ActorDied(Actor actor)
		{
			group.Remove(actor);

			if (Leader == actor)
				newLeader();
		}

		public bool LeaderCheckTarget(Target target)
		{
			return Leader.Bot.CheckTarget(target);
		}

		public void SetNewTarget(Target target)
		{
			foreach (var member in group)
				member.Bot.Target = target;
		}

		void newLeader()
		{
			if (group.Count == 0)
				return;

			Leader = group[0];
			group.Remove(Leader);
		}
	}
}
