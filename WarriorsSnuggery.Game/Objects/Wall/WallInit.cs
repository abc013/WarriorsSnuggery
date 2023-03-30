using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class WallInit : TextNodeInitializer
	{
		public readonly uint ID;

		public readonly short TypeID;

		public readonly WPos Position;
		public readonly short Health;

		// MapFormat < 4
		public WallInit(uint id, WPos position, short type, short health) : base(new List<TextNode>())
		{
			ID = id;

			TypeID = type;
			Position = position;
			Health = health;
		}

		public WallInit(uint id, List<TextNode> nodes, int mapFormat) : base(nodes)
		{
			ID = id;

			TypeID = Convert("Type", (short)-1);
			Position = Convert("Position", WPos.Zero);
			Health = Convert("Health", (short)0);
		}
	}
}
