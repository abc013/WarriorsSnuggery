using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects
{
	public static class TerrainCache
	{
		public static readonly Dictionary<ushort, TerrainType> Types = new Dictionary<ushort, TerrainType>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
			{
				var id = ushort.Parse(node.Key);
				Types.Add(id, new TerrainType(id, node.Children));
			}
		}

		public static Terrain Create(World world, MPos position, ushort ID)
		{
			if (!Types.ContainsKey(ID))
				throw new MissingInfoException(ID.ToString());

			return new Terrain(world, position, Types[ID]);
		}
	}
}
