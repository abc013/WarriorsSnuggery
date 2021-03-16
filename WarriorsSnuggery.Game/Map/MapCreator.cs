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

		public static void LoadMaps(string directory, string file)
		{
			foreach (MissionType type in Enum.GetValues(typeof(MissionType)))
				mapTypes.Add(type, new List<MapType>());

			var mapNodes = TextNodeLoader.FromFile(directory, file);

			foreach (var mapNode in mapNodes)
			{
				var type = MapType.FromRules(mapNode);

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
