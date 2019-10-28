using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarriorsSnuggery.Objects.Conditions
{
	public class Condition
	{
		public readonly bool Negate;
		public readonly string Type;
		public Condition(string input)
		{
			if (input.StartsWith("!"))
			{
				Negate = true;
				input = input.Remove(0, 1);
			}

			Type = input;
		}

		public bool True(Actor actor)
		{
			return actor.World.Game.ConditionManager.CheckCondition(this, actor);
		}
	}
}
