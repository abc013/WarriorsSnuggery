using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Maps
{
	[Desc("These rules contain information about a map that can be generated.")]
	public class MapInfo
	{
		public readonly string Name;

		[Desc("Determines when to use the map.")]
		public readonly MissionType[] MissionTypes = new MissionType[0];
		[Desc("Selection of possible objectives on this map. One of them will be chosen randomly when generating the map.")]
		public readonly ObjectiveType[] AvailableObjectives = new[] { ObjectiveType.NONE };

		[Desc("Single level where this map must be generated.")]
		public readonly int Level = -1;
		[Desc("Level from which this map can be selected to get generated.")]
		public readonly int FromLevel = 0;
		[Desc("Level until this map can be selected to get generated.")]
		public readonly int ToLevel = int.MaxValue;

		[Desc("Piece that has to be generated on the map at the coordinates 0,0.")]
		public readonly string OverridePiece = string.Empty;
		[Desc("Custom size for the map.")]
		public readonly MPos CustomSize = MPos.Zero;
		[Desc("Spawn point of the player.")]
		public readonly MPos SpawnPoint = new MPos(-1, -1);
		[Desc("Allows the use of weapons on the map.")]
		public readonly bool AllowWeapons = true;

		[Desc("Ambient color of the map.")]
		public readonly Color Ambient = Color.White;
		[Desc("Wall type to use when surrounding the map with walls.")]
		public readonly int Wall = 0;

		[Desc("Terrain Generator as basis. Required for the game to function.")]
		public readonly TerrainGeneratorInfo TerrainGenerationBase = null;
		[Desc("Generators to use.", "Add generators as if they where traits.")]
		public readonly List<MapGeneratorInfo> GeneratorInfos = new List<MapGeneratorInfo>();

		[Desc("Noises that are referenced by the generators.", "Add Noisemaps as if they where traits.")]
		public readonly List<NoiseMapInfo> NoiseMapInfos = new List<NoiseMapInfo>();

		[Desc("Determines the file of a script that will be executed during the game.", "Ending of the filename must be '.cs'.")]
		public readonly string MissionScript;

		[Desc("Variable used to determine wether this map comes from an save. DO NOT ALTER.")]
		public readonly bool IsSave;

		// For the DocWriter
		public MapInfo() { }

		MapInfo(string name, List<MiniTextNode> nodes)
		{
			Name = name;

			var fields = PartLoader.GetFields(this);

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case nameof(TerrainGenerationBase):
						TerrainGenerationBase = new TerrainGeneratorInfo(node.Convert<int>(), node.Children);

						break;
					case "PathGeneration":
						GeneratorInfos.Add(new PathGeneratorInfo(node.Convert<int>(), node.Children));

						break;
					case "GridGeneration":
						GeneratorInfos.Add(new GridGeneratorInfo(node.Convert<int>(), node.Children));

						break;
					case "PieceGeneration":
						GeneratorInfos.Add(new PieceGeneratorInfo(node.Convert<int>(), node.Children));

						break;
					case "ImportantPieceGeneration":
						GeneratorInfos.Add(new ImportantPieceGeneratorInfo(node.Convert<int>(), node.Children));

						break;
					case "TerrainGeneration":
						GeneratorInfos.Add(new TerrainGeneratorInfo(node.Convert<int>(), node.Children));

						break;
					case "PatrolGeneration":
						GeneratorInfos.Add(new PatrolGeneratorInfo(node.Convert<int>(), node.Children));

						break;
					case "NoiseMap":
						NoiseMapInfos.Add(new NoiseMapInfo(node.Convert<int>(), node.Children));

						break;
					default:
						PartLoader.SetValue(this, fields, node);

						break;
				}
			}

			// For the documentation
			if (name == "empty")
				return;

			if (TerrainGenerationBase == null)
				throw new MissingNodeException(name, "BaseTerrainGeneration");

			// The highest value has the highest priority
			GeneratorInfos = GeneratorInfos.OrderByDescending(g => g.ID).ToList();
		}

		MapInfo(string overridePiece, int wall, MPos customSize, Color ambient, MissionType[] missionTypes, ObjectiveType[] availableObjectives, int level, int fromLevel, int toLevel, TerrainGeneratorInfo baseTerrainGeneration, List<MapGeneratorInfo> genInfos, MPos spawnPoint, bool isSave, bool allowWeapons, string missionScript)
		{
			OverridePiece = overridePiece;
			Wall = wall;
			CustomSize = customSize;
			Ambient = ambient;
			MissionTypes = missionTypes;
			AvailableObjectives = availableObjectives;
			Level = level;
			FromLevel = fromLevel;
			ToLevel = toLevel;
			TerrainGenerationBase = baseTerrainGeneration;
			GeneratorInfos = genInfos;
			SpawnPoint = spawnPoint;
			IsSave = isSave;
			AllowWeapons = allowWeapons;
			MissionScript = missionScript;
		}

		public static MapInfo FromRules(MiniTextNode parent)
		{
			return new MapInfo(parent.Key, parent.Children);
		}

		public static MapInfo FromSave(GameStatistics stats)
		{
			var size = RuleReader.FromFile(FileExplorer.Saves, stats.MapSaveName + ".yaml").First(n => n.Key == "Size").Convert<MPos>();

			var type = MapCreator.GetType(stats.CurrentMapType);
			var mapGeneratorInfos = type == null ? new List<MapGeneratorInfo>() : type.GeneratorInfos;

			return new MapInfo(stats.MapSaveName, 0, size, Color.White, new[] { stats.CurrentMission }, new[] { stats.CurrentObjective }, -1, 0, int.MaxValue, new TerrainGeneratorInfo(0, new List<MiniTextNode>()), mapGeneratorInfos, MPos.Zero, true, true, stats.Script);
		}

		public static MapInfo FromPiece(Piece piece, MissionType type = MissionType.TEST, ObjectiveType objective = ObjectiveType.NONE)
		{
			return new MapInfo(piece.InnerName, 0, piece.Size, Color.White, new[] { type }, new[] { objective }, -1, 0, int.MaxValue, new TerrainGeneratorInfo(0, new List<MiniTextNode>()), new List<MapGeneratorInfo>(), MPos.Zero, false, true, null);
		}
	}

	public class MapCreator
	{
		static readonly Dictionary<string, MapInfo> mapsNames = new Dictionary<string, MapInfo>();
		static readonly Dictionary<MissionType, List<MapInfo>> mapsTypes = new Dictionary<MissionType, List<MapInfo>>();

		public static void LoadMaps(string directory, string file)
		{
			foreach (MissionType type in Enum.GetValues(typeof(MissionType)))
				mapsTypes.Add(type, new List<MapInfo>());

			var mapNodes = RuleReader.FromFile(directory, file);

			foreach (var mapNode in mapNodes)
			{
				var map = MapInfo.FromRules(mapNode);

				mapsNames.Add(map.Name, map);

				foreach (var missionType in map.MissionTypes)
					mapsTypes[missionType].Add(map);
			}
		}

		public static MapInfo GetType(string name)
		{
			return mapsNames[name];
		}

		public static string GetName(MapInfo info, GameStatistics stats)
		{
			if (info.IsSave)
				return stats.CurrentMapType;

			return mapsNames.FirstOrDefault(t => t.Value == info).Key;
		}

		public static MapInfo FindMap(MissionType type, int level)
		{
			// TODO: make this dependent on seed
			var random = Program.SharedRandom;

			var levels = mapsTypes[type];

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
