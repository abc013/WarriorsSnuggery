namespace WarriorsSnuggery.Objects.Conditions
{
	public class Condition
	{
		public readonly bool Negate;
		public readonly string Type;
		readonly Condition[] children;
		readonly Operation operation;

		enum Operation
		{
			NONE,
			AND,
			OR
		}

		public Condition(string input)
		{
			input = input.Trim();
			if (input.Contains("||"))
			{
				operation = Operation.OR;
				var split = input.LastIndexOf("||");
				children = new Condition[2];
				children[0] = new Condition(input.Substring(0, split));
				children[1] = new Condition(input.Substring(split + 2));
			}
			else if (input.Contains("&&"))
			{
				operation = Operation.AND;
				var split = input.LastIndexOf("&&");
				children = new Condition[2];
				children[0] = new Condition(input.Substring(0, split));
				children[1] = new Condition(input.Substring(split + 2));
			}
			else
			{
				if (input.StartsWith("!"))
				{
					Negate = true;
					input = input.Remove(0, 1);
				}

				Type = input;
			}
		}

		public bool True(Actor actor)
		{
			return operation switch
			{
				Operation.AND => children[0].True(actor) && children[1].True(actor),
				Operation.OR => children[0].True(actor) || children[1].True(actor),
				_ => actor.World.Game.ConditionManager.CheckCondition(this, actor),
			};
		}

		public override string ToString()
		{
			if (children == null)
				return (Negate ? "!" : "") + Type;

			return children[0].ToString() + (operation == Operation.AND ? "&&" : "||") + children[1].ToString();
		}
	}
}
