using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class WeaponInit
	{
		public readonly uint ID;

		public readonly List<TextNode> Nodes;
		public readonly WeaponType Type;

		public readonly CPos Position;
		public readonly int Height;

		public WeaponInit(uint id, List<TextNode> nodes)
		{
			ID = id;
			Nodes = nodes;

			Type = Convert<WeaponType>("Type", null);
			Position = Convert("Position", CPos.Zero);
			Height = Convert("Height", 0);
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
