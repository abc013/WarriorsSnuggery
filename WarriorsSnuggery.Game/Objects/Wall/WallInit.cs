using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class WallInit
	{
		public readonly uint ID;

		public readonly List<TextNode> Nodes;
		public readonly short TypeID;

		public readonly WPos Position;
		public readonly short Health;

		// MapFormat < 4
		public WallInit(uint id, WPos position, short type, short health)
		{
			ID = id;

			TypeID = type;
			Position = position;
			Health = health;
		}

		public WallInit(uint id, List<TextNode> nodes, int mapFormat)
		{
			ID = id;
			Nodes = nodes;

			TypeID = Convert("Type", (short)-1);
			Position = Convert("Position", WPos.Zero);
			Health = Convert("Health", (short)0);
		}

		public T Convert<T>(string rule, T @default)
		{
			var node = Nodes.FirstOrDefault(n => n.Key == rule);
			if (node != null)
				return node.Convert<T>();

			return @default;
		}
	}
}
