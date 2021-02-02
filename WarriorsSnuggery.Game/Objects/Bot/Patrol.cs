using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Objects.Bot
{
	public class Patrol
	{
		public bool IsEmpty => livingActors.Any();
		readonly List<Actor> livingActors = new List<Actor>();
		public Actor Leader;

		public Patrol(List<Actor> actors, Actor leader = null)
		{
			livingActors.AddRange(actors.Where(a => a.IsAlive));

			if (leader != null)
				Leader = leader;
			else
				newLeader();
		}

		public void ActorDied(Actor actor)
		{
			livingActors.Remove(actor);

			if (Leader == actor)
				newLeader();
		}

		void newLeader()
		{
			if (IsEmpty)
				return;
			
			Leader = livingActors[0];
		}
	}
}
