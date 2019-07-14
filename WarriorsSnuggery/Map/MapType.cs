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

		public readonly string[] Entrances;
		public readonly string[] Exits;

		public readonly Dictionary<string, MPos> ImportantParts = new Dictionary<string, MPos>();

		public readonly TerrainGenerationType[] TerrainGeneration;
		public readonly TerrainGenerationType BaseTerrainGeneration;

		public readonly PathGeneratorInfo[] PathGeneration;
		public readonly GridGeneratorInfo[] GridGeneration;

		public readonly StructureGenerationType[] StructureGeneration;

		public readonly EnemyWaveGenerationType[] WaveGeneration;

		public readonly int Wall;

		public readonly MPos CustomSize;
		public readonly Color Ambient;

		public readonly MPos SpawnPoint;

		public readonly bool FromSave;
		public readonly bool AllowWeapons;

		public MapType(string[] entrances, string[] exits, Dictionary<string, MPos> importantParts, int wall, MPos customSize, Color ambient, GameType defaultType, GameMode[] defaultModes, int level, int fromLevel, TerrainGenerationType baseTerrainGeneration, TerrainGenerationType[] terrainGeneration, PathGeneratorInfo[] pathGeneration, GridGeneratorInfo[] gridGeneration, StructureGenerationType[] structureGeneration, EnemyWaveGenerationType[] waveGeneration, MPos spawnPoint, bool fromSave, bool allowWeapons)
		{
			DefaultType = defaultType;
			DefaultModes = defaultModes;
			Level = level;
			FromLevel = fromLevel;
			Entrances = entrances;
			Exits = exits;
			ImportantParts = importantParts;
			Wall = wall;
			CustomSize = customSize;
			Ambient = ambient;
			BaseTerrainGeneration = baseTerrainGeneration;
			TerrainGeneration = terrainGeneration;
			PathGeneration = pathGeneration;
			GridGeneration = gridGeneration;
			StructureGeneration = structureGeneration;
			WaveGeneration = waveGeneration;
			SpawnPoint = spawnPoint;
			FromSave = fromSave;
			AllowWeapons = allowWeapons;
		}

		public string RandomEntrance(Random random)
		{
			return Entrances[random.Next(Entrances.Length)];
		}

		public string RandomExit(Random random)
		{
			return Exits[random.Next(Exits.Length)];
		}

		public static MapType MapTypeFromSave(GameStatistics stats)
		{
			var piece = stats.SaveName + "_map";
			var size = loadPieceSize(RuleReader.Read(FileExplorer.Saves, stats.SaveName + "_map.yaml"));

			var dict = new Dictionary<string, MPos>
			{ { piece, MPos.Zero } };

			return new MapType(new string[] { }, new string[] { }, dict, 0, size, Color.White, GameType.NORMAL, new[] { stats.Mode }, -1, 0, TerrainGenerationType.Empty(), new TerrainGenerationType[0], new PathGeneratorInfo[0], new GridGeneratorInfo[0], new StructureGenerationType[0], new EnemyWaveGenerationType[0], MPos.Zero, true, true);
		}

		public static MapType EditorMapTypeFromPiece(string piece, MPos size)
		{
			var dict = new Dictionary<string, MPos>
			{ { piece, MPos.Zero } };
			return new MapType(new string[] { }, new string[] { }, dict, 0, size, Color.White, GameType.EDITOR, new[] { GameMode.NONE }, -1, 0, TerrainGenerationType.Empty(), new TerrainGenerationType[0], new PathGeneratorInfo[0], new GridGeneratorInfo[0], new StructureGenerationType[0], new EnemyWaveGenerationType[0], MPos.Zero, false, true);
		}

		public static MapType ConvertGameType(MapType map, GameType type)
		{
			return new MapType(map.Entrances, map.Exits, map.ImportantParts, map.Wall, map.CustomSize, map.Ambient, type, map.DefaultModes, map.Level, map.FromLevel, map.BaseTerrainGeneration, map.TerrainGeneration, map.PathGeneration, map.GridGeneration, map.StructureGeneration, map.WaveGeneration, map.SpawnPoint, map.FromSave, map.AllowWeapons);
		}

		static MPos loadPieceSize(List<MiniTextNode> nodes)
		{
			var node = nodes.FirstOrDefault(n => n.Key == "Size");

			return node != null ? node.Convert<MPos>() : MPos.Zero;
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
				var exits = new string[0];
				var entrances = new string[0];
				var importantParts = new Dictionary<string, MPos>();
				var wall = 0;
				var ambient = Color.White;
				var customSize = MPos.Zero;
				var terrainGen = new List<TerrainGenerationType>();
				var structureGen = new List<StructureGenerationType>();
				var pathGen = new List<PathGeneratorInfo>();
				var gridGen = new List<GridGeneratorInfo>();
				var waveGen = new List<EnemyWaveGenerationType>();
				var spawnPoint = new MPos(-1, -1);
				TerrainGenerationType baseterrain = null;
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
						case "ActiveFromLevel":
							fromLevel = child.Convert<int>();

							break;
						case "Level":
							level = child.Convert<int>();

							break;
						case "MainPieces":
							foreach (var piece in child.Children)
							{
								importantParts.Add(piece.Key, piece.Convert<MPos>());
							}

							break;
						case "Exits":
							exits = child.Convert<string[]>();

							break;
						case "SpawnPoint":
							spawnPoint = child.Convert<MPos>();

							break;
						case "Entrances":
							entrances = child.Convert<string[]>();

							break;
						case "Wall":
							wall = child.Convert<int>();

							break;
						case "BaseTerrainGeneration":
							baseterrain = TerrainGenerationType.GetType(0, child.Children.ToArray());

							break;
						case "TerrainGeneration":
							terrainGen.Add(TerrainGenerationType.GetType(child.Convert<int>(), child.Children.ToArray()));

							break;
						case "PathGeneration":
							pathGen.Add(new PathGeneratorInfo(child.Convert<int>(), child.Children.ToArray()));

							break;
						case "GridGeneration":
							gridGen.Add(new GridGeneratorInfo(child.Convert<int>(), child.Children.ToArray()));

							break;
						case "StructureGeneration":
							structureGen.Add(StructureGenerationType.GetType(child.Convert<int>(), child.Children.ToArray()));

							break;
						case "WaveGeneration":
							waveGen.Add(EnemyWaveGenerationType.GetType(child.Convert<int>(), child.Children.ToArray()));

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

				if (baseterrain == null)
					throw new YamlMissingNodeException(terrain.Key, "BaseTerrainGeneration");

				AddType(new MapType(entrances, exits, importantParts, wall, customSize, ambient, playType, playModes, level, fromLevel, baseterrain, terrainGen.ToArray(), pathGen.ToArray(), gridGen.ToArray(), structureGen.ToArray(), waveGen.ToArray(), spawnPoint, false, allowWeapons), name);
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
