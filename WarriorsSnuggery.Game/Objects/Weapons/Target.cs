namespace WarriorsSnuggery.Objects.Weapons
{
	public enum TargetType : byte
	{
		ACTOR,
		POSITION
	}

	public class Target
	{
		public readonly TargetType Type;

		public CPos Position
		{
			get
			{
				if (Type == TargetType.POSITION)
					return position;
				else
					return Actor.Position;
			}
		}
		readonly CPos position;
		public int Height
		{
			get
			{
				if (Type == TargetType.POSITION)
					return height;
				else
					return Actor.Height;
			}
		}
		readonly int height;

		public readonly Actor Actor;

		public Target(CPos target, int height)
		{
			position = target;
			this.height = height;
			Type = TargetType.POSITION;
		}

		public Target(Actor target)
		{
			Actor = target;
			Type = TargetType.ACTOR;
		}
	}
}
