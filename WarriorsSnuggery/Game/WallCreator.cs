using System.Collections.Generic;

namespace WarriorsSnuggery.Objects
{
	public static class WallCreator
	{
		public static readonly Dictionary<int, WallType> Types = new Dictionary<int, WallType>();

		public static void Load(string directory, string file)
		{
			var walls = RuleReader.Read(directory, file);

			foreach (var wall in walls)
			{
				var id = int.Parse(wall.Key);

				Types.Add(id, new WallType(id, wall.Children.ToArray()));
			}
		}

		public static Wall Create(MPos position, WallLayer layer, int ID)
		{
			if (!Types.ContainsKey(ID))
				throw new MissingInfoException(ID.ToString());

			return new Wall(position, layer, Types[ID]);
		}
	}
}
