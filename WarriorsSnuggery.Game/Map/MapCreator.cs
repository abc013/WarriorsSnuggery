using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Maps
{
	public static class MapCreator
	{
		static readonly Dictionary<string, MapType> mapNames = new Dictionary<string, MapType>();
		static readonly Dictionary<MissionType, List<MapType>> mapTypes = new Dictionary<MissionType, List<MapType>>();

		public static void LoadMaps()
		{
			var rules = TextNodeLoader.FromFile(FileExplorer.Maps, "maps.yaml");
			foreach (var rule in rules)
			{
				var data = new (string directory, string file)[rule.Children.Count];
				for (int i = 0; i < data.Length; i++)
				{
					var file = rule.Children[i].Key;
					data[i] = (FileExplorer.FindPath(FileExplorer.Maps, file, string.Empty), file);
				}

				var loader = new ComplexTextNodeLoader(rule.Key);
				loader.Load(data);

				LoadMaps(loader.Finish());
			}
		}

		public static void LoadMaps(List<TextNode> nodes)
		{
			foreach (MissionType type in Enum.GetValues(typeof(MissionType)))
				mapTypes.Add(type, new List<MapType>());

			foreach (var node in nodes)
			{
				var type = MapType.FromRules(node);

				mapNames.Add(type.Name, type);

				foreach (var missionType in type.MissionTypes)
					mapTypes[missionType].Add(type);
			}
		}

		public static MapType GetType(string name)
		{
			return mapNames[name];
		}

		public static string GetName(MapType type)
		{
			return mapNames.FirstOrDefault(t => t.Value == type).Key;
		}

		public static MapType FindMap(MissionType type, GameSave save)
		{
			return FindMap(type, save.Level, new Random(save.Seed + save.Level));
		}

		public static MapType FindMap(MissionType type, int level, Random random)
		{
			var levels = mapTypes[type];

			var explicitLevels = levels.Where(a => level == a.Level);
			if (explicitLevels.Any())
				return explicitLevels.ElementAt(random.Next(explicitLevels.Count()));

			var implicitLevels = levels.Where(a => level >= a.FromLevel && level <= a.ToLevel && a.FromLevel >= 0 && a.Level == -1);
			if (!implicitLevels.Any())
				throw new MissingFieldException($"There are no available maps of type '{type}' (current level: {level}).");

			return implicitLevels.ElementAt(random.Next(implicitLevels.Count()));
		}
	}
}
