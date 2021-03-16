using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects
{
	public static class TerrainCreator
	{
		public static readonly Dictionary<ushort, TerrainType> Types = new Dictionary<ushort, TerrainType>();

		public static void LoadTypes(string directory, string file)
		{
			var terrains = TextNodeLoader.FromFile(directory, file);

			foreach (var terrain in terrains)
			{
				var id = ushort.Parse(terrain.Key);
				Types.Add(id, new TerrainType(id, terrain.Children));
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
