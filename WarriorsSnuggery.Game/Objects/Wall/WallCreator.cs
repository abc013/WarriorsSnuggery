using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps.Layers;

namespace WarriorsSnuggery.Objects
{
	public static class WallCreator
	{
		public static readonly Dictionary<short, WallType> Types = new Dictionary<short, WallType>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
			{
				var id = short.Parse(node.Key);
				Types.Add(id, new WallType(id, node.Children));
			}
		}

		public static Wall Create(MPos position, WallLayer layer, short ID)
		{
			if (!Types.ContainsKey(ID))
				throw new MissingInfoException(ID.ToString());

			return new Wall(position, layer, Types[ID]);
		}
	}
}
