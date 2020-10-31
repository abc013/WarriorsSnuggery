using System.Collections.Generic;

namespace WarriorsSnuggery.Objects
{
	public static class WallCreator
	{
		public static readonly Dictionary<short, WallType> Types = new Dictionary<short, WallType>();

		public static void Load(string directory, string file)
		{
			var walls = RuleReader.FromFile(directory, file);

			foreach (var wall in walls)
			{
				var id = short.Parse(wall.Key);

				Types.Add(id, new WallType(id, wall.Children));
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
