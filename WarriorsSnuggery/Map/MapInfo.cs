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

		[Desc("Type of the map.")]
		public readonly GameType DefaultType = GameType.NORMAL;
		[Desc("Possible modes on this map.")]
		public readonly GameMode[] DefaultModes = new[] { GameMode.NONE };

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
		public readonly bool FromSave;

		public MapInfo(string name, List<MiniTextNode> nodes)
		{
			Name = name;

			var fields = PartLoader.GetFields(this);

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "DefaultModes":
						var modeArray = node.Convert<string[]>();

						DefaultModes = new GameMode[modeArray.Length];
						for (int i = 0; i < DefaultModes.Length; i++)
							DefaultModes[i] = (GameMode)Enum.Parse(typeof(GameMode), modeArray[i]);

						break;
					case "TerrainGenerationBase":
						TerrainGenerationBase = new TerrainGeneratorInfo(node.Convert<int>(), node.Children.ToArray());

						break;
					case "PathGeneration":
						GeneratorInfos.Add(new PathGeneratorInfo(node.Convert<int>(), node.Children.ToArray()));

						break;
					case "GridGeneration":
						GeneratorInfos.Add(new GridGeneratorInfo(node.Convert<int>(), node.Children.ToArray()));

						break;
					case "PieceGeneration":
						GeneratorInfos.Add(new PieceGeneratorInfo(node.Convert<int>(), node.Children.ToArray()));

						break;
					case "ImportantPieceGeneration":
						GeneratorInfos.Add(new ImportantPieceGeneratorInfo(node.Convert<int>(), node.Children.ToArray()));

						break;
					case "TerrainGeneration":
						GeneratorInfos.Add(new TerrainGeneratorInfo(node.Convert<int>(), node.Children.ToArray()));

						break;
					case "PatrolGeneration":
						GeneratorInfos.Add(new PatrolGeneratorInfo(node.Convert<int>(), node.Children.ToArray()));

						break;
					case "NoiseMap":
						NoiseMapInfos.Add(new NoiseMapInfo(node.Convert<int>(), node.Children.ToArray()));

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
				throw new YamlMissingNodeException(name, "BaseTerrainGeneration");

			// The highest value has the highest priority
			GeneratorInfos = GeneratorInfos.OrderByDescending(g => g.ID).ToList();
		}

		public MapInfo(string overridePiece, int wall, MPos customSize, Color ambient, GameType defaultType, GameMode[] defaultModes, int level, int fromLevel, int toLevel, TerrainGeneratorInfo baseTerrainGeneration, List<MapGeneratorInfo> genInfos, MPos spawnPoint, bool fromSave, bool allowWeapons, string missionScript)
		{
			OverridePiece = overridePiece;
			Wall = wall;
			CustomSize = customSize;
			Ambient = ambient;
			DefaultType = defaultType;
			DefaultModes = defaultModes;
			Level = level;
			FromLevel = fromLevel;
			ToLevel = toLevel;
			TerrainGenerationBase = baseTerrainGeneration;
			GeneratorInfos = genInfos;
			SpawnPoint = spawnPoint;
			FromSave = fromSave;
			AllowWeapons = allowWeapons;
			MissionScript = missionScript;
		}

		public static MapInfo MapTypeFromSave(GameStatistics stats)
		{
			var piece = stats.SaveName + "_map";
			var size = RuleReader.Read(FileExplorer.Saves, stats.SaveName + "_map.yaml").First(n => n.Key == "Size").Convert<MPos>();
			var type = MapCreator.GetType(stats.CurrentMapType);
			var mapGeneratorInfos = type == null ? new List<MapGeneratorInfo>() : type.GeneratorInfos;
			return new MapInfo(piece, 0, size, Color.White, stats.CurrentType, new[] { stats.CurrentMode }, -1, 0, int.MaxValue, new TerrainGeneratorInfo(0, new MiniTextNode[0]), mapGeneratorInfos, MPos.Zero, true, true, stats.Script);
		}

		public static MapInfo EditorMapTypeFromPiece(string piece, MPos size)
		{
			return new MapInfo(piece, 0, size, Color.White, GameType.EDITOR, new[] { GameMode.NONE }, -1, 0, int.MaxValue, new TerrainGeneratorInfo(0, new MiniTextNode[0]), new List<MapGeneratorInfo>(), MPos.Zero, false, true, null);
		}

		public static MapInfo ConvertGameType(MapInfo map, GameType type)
		{
			return new MapInfo(map.OverridePiece, map.Wall, map.CustomSize, map.Ambient, type, map.DefaultModes, map.Level, map.FromLevel, map.ToLevel, map.TerrainGenerationBase, map.GeneratorInfos, map.SpawnPoint, map.FromSave, map.AllowWeapons, null);
		}
	}

	public class MapCreator
	{
		static readonly Dictionary<string, MapInfo> mapsNames = new Dictionary<string, MapInfo>();
		static readonly Dictionary<GameType, List<MapInfo>> mapsTypes = new Dictionary<GameType, List<MapInfo>>();

		public static void LoadMaps(string directory, string file)
		{
			foreach (GameType type in Enum.GetValues(typeof(GameType)))
				mapsTypes.Add(type, new List<MapInfo>());

			var mapNodes = RuleReader.Read(directory, file);

			foreach (var mapNode in mapNodes)
			{
				var map = new MapInfo(mapNode.Key, mapNode.Children);

				mapsNames.Add(mapNode.Key, map);

				mapsTypes[map.DefaultType].Add(map);
			}
		}

		public static MapInfo GetType(string name)
		{
			return mapsNames[name];
		}

		public static string GetName(MapInfo info, GameStatistics stats)
		{
			if (info.FromSave)
				return stats.CurrentMapType;

			return mapsNames.FirstOrDefault(t => t.Value == info).Key;
		}

		public static MapInfo FindMap(GameType type, int level)
		{
			if (type == GameType.TUTORIAL)
				return findTutorial();

			var levels = mapsTypes[type];

			var explicitLevels = levels.Where(a => level == a.Level);
			if (explicitLevels.Any())
				return explicitLevels.ElementAt(Program.SharedRandom.Next(explicitLevels.Count()));

			var implicitLevels = levels.Where(a => level >= a.FromLevel && level <= a.ToLevel && a.FromLevel >= 0 && a.Level == -1);
			if (!implicitLevels.Any())
				throw new MissingFieldException(string.Format("There are no available Main Maps (Level:{0}).", level));

			return implicitLevels.ElementAt(Program.SharedRandom.Next(implicitLevels.Count()));
		}

		static MapInfo findTutorial()
		{
			var implicitLevels = mapsTypes[GameType.TUTORIAL];

			if (!implicitLevels.Any())
				throw new MissingFieldException(string.Format("There are no Tutorial Maps available."));

			return implicitLevels.ElementAt(Program.SharedRandom.Next(implicitLevels.Count()));
		}
	}
}
