using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Maps
{
	[Desc("These rules contain information about a map that can be generated.")]
	public class MapInfo
	{
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
		[Desc("Generators to use. To add, just do it like traits.")]
		public readonly MapGeneratorInfo[] GeneratorInfos = new MapGeneratorInfo[0];

		[Desc("Determines the file of a script that will be executed during the game.", "Ending of the filename must be '.cs'.")]
		public readonly string MissionScript;

		[Desc("Variable used to determine wether this map comes from an save. DO NOT ALTER.")]
		public readonly bool FromSave;

		public MapInfo(string overridePiece, int wall, MPos customSize, Color ambient, GameType defaultType, GameMode[] defaultModes, int level, int fromLevel, int toLevel, TerrainGeneratorInfo baseTerrainGeneration, MapGeneratorInfo[] genInfos, MPos spawnPoint, bool fromSave, bool allowWeapons, string missionScript)
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

		public MapInfo(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}

		public static MapInfo MapTypeFromSave(GameStatistics stats)
		{
			var piece = stats.SaveName + "_map";
			var size = RuleReader.Read(FileExplorer.Saves, stats.SaveName + "_map.yaml").First(n => n.Key == "Size").Convert<MPos>();
			var type = MapCreator.GetType(stats.MapType);
			var mapGeneratorInfos = type == null ? new MapGeneratorInfo[0] : type.GeneratorInfos;
			return new MapInfo(piece, 0, size, Color.White, stats.Type, new[] { stats.Mode }, -1, 0, int.MaxValue, new TerrainGeneratorInfo(0, new MiniTextNode[0]), mapGeneratorInfos, MPos.Zero, true, true, null);
		}

		public static MapInfo EditorMapTypeFromPiece(string piece, MPos size)
		{
			return new MapInfo(piece, 0, size, Color.White, GameType.EDITOR, new[] { GameMode.NONE }, -1, 0, int.MaxValue, new TerrainGeneratorInfo(0, new MiniTextNode[0]), new MapGeneratorInfo[0], MPos.Zero, false, true, null);
		}

		public static MapInfo ConvertGameType(MapInfo map, GameType type)
		{
			return new MapInfo(map.OverridePiece, map.Wall, map.CustomSize, map.Ambient, type, map.DefaultModes, map.Level, map.FromLevel, map.ToLevel, map.TerrainGenerationBase, map.GeneratorInfos, map.SpawnPoint, map.FromSave, map.AllowWeapons, null);
		}
	}

	public class MapCreator
	{
		public static void LoadTypes(string directory, string file)
		{
			var infos = RuleReader.Read(directory, file);

			foreach (var terrain in infos)
			{
				var playType = GameType.NORMAL;
				var playModes = new[] { GameMode.NONE };
				var level = -1;
				var fromLevel = 0;
				var toLevel = int.MaxValue;
				var name = terrain.Key;
				var wall = 0;
				var ambient = Color.White;
				var customSize = MPos.Zero;
				var genInfos = new List<MapGeneratorInfo>();
				var spawnPoint = new MPos(-1, -1);
				TerrainGeneratorInfo baseterrain = null;
				var allowWeapons = true;
				var missionScript = string.Empty;

				foreach (var child in terrain.Children)
				{
					switch (child.Key)
					{
						case "PlayType":
							playType = child.Convert<GameType>();

							break;
						case "PlayModes":
							var modeArray = child.Convert<string[]>();

							playModes = new GameMode[modeArray.Length];
							for (int i = 0; i < playModes.Length; i++)
							{
								playModes[i] = (GameMode)Enum.Parse(typeof(GameMode), modeArray[i]);
							}
							break;
						case "FromLevel":
							fromLevel = child.Convert<int>();

							break;
						case "ToLevel":
							toLevel = child.Convert<int>();

							break;
						case "Level":
							level = child.Convert<int>();

							break;
						case "SpawnPoint":
							spawnPoint = child.Convert<MPos>();

							break;
						case "Wall":
							wall = child.Convert<int>();

							break;
						case "TerrainGenerationBase":
							baseterrain = new TerrainGeneratorInfo(child.Convert<int>(), child.Children.ToArray());

							break;
						case "PathGeneration":
							genInfos.Add(new PathGeneratorInfo(child.Convert<int>(), child.Children.ToArray()));

							break;
						case "GridGeneration":
							genInfos.Add(new GridGeneratorInfo(child.Convert<int>(), child.Children.ToArray()));

							break;
						case "PieceGeneration":
							genInfos.Add(new PieceGeneratorInfo(child.Convert<int>(), child.Children.ToArray()));

							break;
						case "ImportantPieceGeneration":
							genInfos.Add(new ImportantPieceGeneratorInfo(child.Convert<int>(), child.Children.ToArray()));

							break;
						case "TerrainGeneration":
							genInfos.Add(new TerrainGeneratorInfo(child.Convert<int>(), child.Children.ToArray()));

							break;
						case "PatrolGeneration":
							genInfos.Add(new PatrolGeneratorInfo(child.Convert<int>(), child.Children.ToArray()));

							break;
						case "CustomSize":
							customSize = child.Convert<MPos>();

							break;
						case "Ambient":
							ambient = child.Convert<Color>();

							break;
						case "AllowWeapons":
							allowWeapons = child.Convert<bool>();

							break;
						case "MissionScript":
							missionScript = child.Value.Trim();

							break;
						default:
							throw new YamlUnknownNodeException(child.Key, terrain.Key);
					}
				}

				// The highest value has the highest priority
				genInfos = genInfos.OrderByDescending(g => g.ID).ToList();

				if (baseterrain == null)
					throw new YamlMissingNodeException(terrain.Key, "BaseTerrainGeneration");

				AddType(new MapInfo(string.Empty, wall, customSize, ambient, playType, playModes, level, fromLevel, toLevel, baseterrain, genInfos.ToArray(), spawnPoint, false, allowWeapons, missionScript), name);
			}
		}

		static readonly Dictionary<string, MapInfo> types = new Dictionary<string, MapInfo>();

		public static void AddType(MapInfo type, string name)
		{
			types.Add(name, type);
		}

		public static MapInfo GetType(string name)
		{
			return types[name];
		}

		public static string GetName(MapInfo info, GameStatistics stats)
		{
			if (info.FromSave)
				return stats.MapType;

			return types.FirstOrDefault(t => t.Value == info).Key;
		}

		public static MapInfo FindMainMenuMap(int level)
		{
			var levels = types.Values.Where(a => a.DefaultType == GameType.MAINMENU);

			var explicitLevels = levels.Where(a => level == a.Level);
			if (explicitLevels.Any())
				return explicitLevels.ElementAt(Program.SharedRandom.Next(explicitLevels.Count()));

			var implicitLevels = levels.Where(a => level >= a.FromLevel && level <= a.ToLevel && a.FromLevel >= 0 && a.Level == -1);
			if (!implicitLevels.Any())
				throw new MissingFieldException(string.Format("There are no available Main Maps (Level:{0}).", level));

			return implicitLevels.ElementAt(Program.SharedRandom.Next(implicitLevels.Count()));
		}

		public static MapInfo FindMainMap(int level)
		{
			var levels = types.Values.Where(a => a.DefaultType == GameType.MENU);

			var mainLevels = levels.Where(a => level == a.Level);
			if (mainLevels.Any())
				return mainLevels.ElementAt(Program.SharedRandom.Next(mainLevels.Count()));

			var mainTypes = levels.Where(a => level >= a.FromLevel && level <= a.ToLevel && a.FromLevel >= 0 && a.Level == -1);
			if (!mainTypes.Any())
				throw new MissingFieldException(string.Format("There are no available Main Maps (Level:{0}).", level));

			return mainTypes.ElementAt(Program.SharedRandom.Next(mainTypes.Count()));
		}

		public static MapInfo FindMap(int level)
		{
			var levels = types.Values.Where(a => a.DefaultType == GameType.NORMAL);

			var explicitLevels = levels.Where(a => level == a.Level);
			if (explicitLevels.Any())
				return explicitLevels.ElementAt(Program.SharedRandom.Next(explicitLevels.Count()));

			var mainTypes = levels.Where(a => level >= a.FromLevel && level <= a.ToLevel && a.FromLevel >= 0 && a.Level == -1);
			if (!mainTypes.Any())
				throw new MissingFieldException(string.Format("There are no Maps available."));

			return mainTypes.ElementAt(Program.SharedRandom.Next(mainTypes.Count()));
		}

		public static MapInfo FindTutorial()
		{
			var implicitLevels = types.Values.Where(a => a.DefaultType == GameType.TUTORIAL);

			if (!implicitLevels.Any())
				throw new MissingFieldException(string.Format("There are no Tutorial Maps available."));

			return implicitLevels.ElementAt(Program.SharedRandom.Next(implicitLevels.Count()));
		}
	}
}
