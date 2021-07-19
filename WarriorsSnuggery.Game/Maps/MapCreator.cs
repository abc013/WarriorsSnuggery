using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Maps
{
	public static class MapCreator
	{
		public static readonly TypeDictionary<MapType> Types = new TypeDictionary<MapType>();
		static readonly Dictionary<MissionType, List<MapType>> missionMaps = new Dictionary<MissionType, List<MapType>>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (MissionType type in Enum.GetValues(typeof(MissionType)))
				missionMaps.Add(type, new List<MapType>());

			foreach (var node in nodes)
			{
				var type = MapType.FromRules(node);

				Types.Add(type.Name, type);

				foreach (var missionType in type.MissionTypes)
					missionMaps[missionType].Add(type);
			}
		}

		public static MapType FindMap(MissionType type, GameSave save)
		{
			return FindMap(type, save.Level, new Random(save.Seed + save.Level));
		}

		public static MapType FindMap(MissionType type, int level, Random random)
		{
			var levels = missionMaps[type];

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
