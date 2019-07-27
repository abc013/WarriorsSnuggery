using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Maps
{
	public class MapType
	{
		public readonly int Level;
		public readonly int FromLevel;

		public readonly GameType DefaultType;
		public readonly GameMode[] DefaultModes;

		public readonly string OverridePiece;

		public readonly TerrainGeneratorInfo BaseTerrainGeneration;
		public readonly MapGeneratorInfo[] GeneratorInfos;

		public readonly int Wall;

		public readonly MPos CustomSize;
		public readonly Color Ambient;

		public readonly MPos SpawnPoint;

		public readonly bool FromSave;
		public readonly bool AllowWeapons;

		public MapType(string overridePiece, int wall, MPos customSize, Color ambient, GameType defaultType, GameMode[] defaultModes, int level, int fromLevel, TerrainGeneratorInfo baseTerrainGeneration, MapGeneratorInfo[] genInfos, MPos spawnPoint, bool fromSave, bool allowWeapons)
		{
			OverridePiece = overridePiece;
			Wall = wall;
			CustomSize = customSize;
			Ambient = ambient;
			DefaultType = defaultType;
			DefaultModes = defaultModes;
			Level = level;
			FromLevel = fromLevel;
			BaseTerrainGeneration = baseTerrainGeneration;
			GeneratorInfos = genInfos;
			SpawnPoint = spawnPoint;
			FromSave = fromSave;
			AllowWeapons = allowWeapons;
		}

		public static MapType MapTypeFromSave(GameStatistics stats)
		{
			var piece = stats.SaveName + "_map";
			var size = RuleReader.Read(FileExplorer.Saves, stats.SaveName + "_map.yaml").First(n => n.Key == "Size").Convert<MPos>();

			return new MapType(piece, 0, size, Color.White, GameType.NORMAL, new[] { stats.Mode }, -1, 0, new TerrainGeneratorInfo(0, new MiniTextNode[0]), new MapGeneratorInfo[0], MPos.Zero, true, true);
		}

		public static MapType EditorMapTypeFromPiece(string piece, MPos size)
		{
			return new MapType(piece, 0, size, Color.White, GameType.EDITOR, new[] { GameMode.NONE }, -1, 0, new TerrainGeneratorInfo(0, new MiniTextNode[0]), new MapGeneratorInfo[0], MPos.Zero, false, true);
		}

		public static MapType ConvertGameType(MapType map, GameType type)
		{
			return new MapType(map.OverridePiece, map.Wall, map.CustomSize, map.Ambient, type, map.DefaultModes, map.Level, map.FromLevel, map.BaseTerrainGeneration, map.GeneratorInfos, map.SpawnPoint, map.FromSave, map.AllowWeapons);
		}
	}

	public class MapCreator
	{
		public static void LoadTypes(string directory, string file)
		{
			var terrains = RuleReader.Read(directory, file);

			foreach (var terrain in terrains)
			{
				var playType = GameType.NORMAL;
				var playModes = new[] { GameMode.NONE };
				var level = -1;
				var fromLevel = 0;
				var name = terrain.Key;
				var wall = 0;
				var ambient = Color.White;
				var customSize = MPos.Zero;
				var genInfos = new List<MapGeneratorInfo>();
				var spawnPoint = new MPos(-1, -1);
				TerrainGeneratorInfo baseterrain = null;
				var allowWeapons = true;

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
						case "Level":
							level = child.Convert<int>();

							break;
						case "SpawnPoint":
							spawnPoint = child.Convert<MPos>();

							break;
						case "Wall":
							wall = child.Convert<int>();

							break;
						case "BaseTerrainGeneration":
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
						case "CustomSize":
							customSize = child.Convert<MPos>();

							break;
						case "Ambient":
							ambient = child.Convert<Color>();

							break;
						case "AllowWeapons":
							allowWeapons = child.Convert<bool>();

							break;
						default:
							throw new YamlUnknownNodeException(child.Key, terrain.Key);
					}
				}

				// The highest value has the highest priority
				genInfos = genInfos.OrderByDescending(g => g.ID).ToList();

				if (baseterrain == null)
					throw new YamlMissingNodeException(terrain.Key, "BaseTerrainGeneration");

				AddType(new MapType(string.Empty, wall, customSize, ambient, playType, playModes, level, fromLevel, baseterrain, genInfos.ToArray(), spawnPoint, false, allowWeapons), name);
			}
		}

		static readonly Dictionary<string, MapType> types = new Dictionary<string, MapType>();

		public static void AddType(MapType type, string name)
		{
			types.Add(name, type);
		}

		public static MapType GetType(string name)
		{
			return types[name];
		}

		public static MapType FindMainMenuMap(int level)
		{
			var mainLevels = types.Values.Where(a => a.DefaultType == GameType.MAINMENU && level == a.Level).ToList();

			if (mainLevels.Any())
				return mainLevels[Program.SharedRandom.Next(mainLevels.Count())];

			var mainTypes = types.Values.Where(a => a.DefaultType == GameType.MAINMENU && level >= a.FromLevel && a.FromLevel >= 0).ToList();

			if (mainTypes.Count == 0)
				throw new MissingFieldException(string.Format("There are no available Main Maps (Level:{0}).", level));

			return mainTypes[Program.SharedRandom.Next(mainTypes.Count())];
		}

		public static MapType FindMainMap(int level)
		{
			var mainLevels = types.Values.Where(a => a.DefaultType == GameType.MENU && level == a.Level).ToList();

			if (mainLevels.Any())
				return mainLevels[Program.SharedRandom.Next(mainLevels.Count())];

			var mainTypes = types.Values.Where(a => a.DefaultType == GameType.MENU && level >= a.FromLevel && a.FromLevel >= 0).ToList();

			if (mainTypes.Count == 0)
				throw new MissingFieldException(string.Format("There are no available Main Maps (Level:{0}).", level));

			return mainTypes[Program.SharedRandom.Next(mainTypes.Count())];
		}

		public static MapType FindMap(int level)
		{
			var mainLevels = types.Values.Where(a => a.DefaultType == GameType.NORMAL && level == a.Level).ToList();

			if (mainLevels.Any())
				return mainLevels[Program.SharedRandom.Next(mainLevels.Count())];

			var mainTypes = types.Values.Where(a => a.DefaultType == GameType.NORMAL && level >= a.FromLevel && a.FromLevel >= 0).ToList();

			if (mainTypes.Count == 0)
				throw new MissingFieldException(string.Format("There are no Maps available."));

			return mainTypes[Program.SharedRandom.Next(mainTypes.Count())];
		}

		public static MapType FindTutorial()
		{
			var mainTypes = types.Values.Where(a => a.DefaultType == GameType.TUTORIAL).ToList();

			if (mainTypes.Count == 0)
				throw new MissingFieldException(string.Format("There are no Tutorial Maps available."));

			return mainTypes[Program.SharedRandom.Next(mainTypes.Count())];
		}
	}
}
