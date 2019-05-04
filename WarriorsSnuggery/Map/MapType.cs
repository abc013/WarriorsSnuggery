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

		public readonly string[] Parts;
		public readonly string[] Entrances;
		public readonly string[] Exits;

		public readonly Dictionary<string, MPos> ImportantParts = new Dictionary<string, MPos>();

		public readonly TerrainGenerationType[] TerrainGeneration;
		public readonly TerrainGenerationType BaseTerrainGeneration;

		public readonly StructureGenerationType[] StructureGeneration;

		public readonly int Wall;

		public readonly MPos CustomSize;
		public readonly Color Ambient;
		
		public readonly MPos SpawnPoint;

		public MapType(string[] parts, string[] entrances, string[] exits, Dictionary<string, MPos> importantParts, int wall, MPos customSize, Color ambient, GameType defaultType, GameMode[] defaultModes, int level, int fromLevel, TerrainGenerationType baseTerrainGeneration, TerrainGenerationType[] terrainGeneration, StructureGenerationType[] structureGeneration, MPos spawnPoint)
		{
			DefaultType = defaultType;
			DefaultModes = defaultModes;
			Level = level;
			FromLevel = fromLevel;
			Parts = parts;
			Entrances = entrances;
			Exits = exits;
			ImportantParts = importantParts;
			Wall = wall;
			CustomSize = customSize;
			Ambient = ambient;
			BaseTerrainGeneration = baseTerrainGeneration;
			TerrainGeneration = terrainGeneration;
			StructureGeneration = structureGeneration;
			SpawnPoint = spawnPoint;
		}

		public string RandomPiece(Random random)
		{
			return Parts[random.Next(Parts.Length)];
		}

		public string RandomEntrance(Random random)
		{
			return Entrances[random.Next(Entrances.Length)];
		}

		public string RandomExit(Random random)
		{
			return Exits[random.Next(Exits.Length)];
		}

		public static MapType MapTypeFromPiece(string piece, MPos size)
		{
			var dict = new Dictionary<string, MPos>
			{ { piece, MPos.Zero } };
			return new MapType(new string[] { }, new string[] { }, new string[] { }, dict, 0, size, Color.White, GameType.EDITOR, new[] { GameMode.NONE }, -1, 0, TerrainGenerationType.Empty(), new TerrainGenerationType[0], new StructureGenerationType[0], MPos.Zero);
		}

		public static MapType ConvertGameType(MapType map, GameType type)
		{
			return new MapType(map.Parts, map.Entrances, map.Exits, map.ImportantParts, map.Wall, map.CustomSize, map.Ambient, type, map.DefaultModes, map.Level, map.FromLevel, map.BaseTerrainGeneration, map.TerrainGeneration, map.StructureGeneration, map.SpawnPoint);
		}
	}

	public class MapCreator
	{
		public static void LoadTypes(string file)
		{
			var terrains = RuleReader.Read(file);

			foreach (var terrain in terrains)
			{
				var playType = GameType.NORMAL;
				var playModes = new[] { GameMode.NONE };
				var level = -1;
				var fromLevel = 0;
				var name = terrain.Key;
				var parts = new string[0];
				var exits = new string[0];
				var entrances = new string[0];
				var importantParts = new Dictionary<string, MPos>();
				var wall = 0;
				var ambient = Color.White;
				var customSize = MPos.Zero;
				var terrainGen = new List<TerrainGenerationType>();
				var structureGen = new List<StructureGenerationType>();
				var spawnPoint = new MPos(-1, -1);
				TerrainGenerationType baseterrain = null;

				foreach (var child in terrain.Children)
				{
					switch (child.Key)
					{
						case "PlayType":
							playType = (GameType) child.ToEnum(typeof(GameType));

							break;
						case "PlayModes":
							var modeArray = child.ToArray();
							playModes = new GameMode[modeArray.Length];

							for(int i = 0; i < playModes.Length; i++)
							{
								playModes[i] = (GameMode) Enum.Parse(typeof(GameMode), modeArray[i]);
							}
							break;
						case "ActiveFromLevel":
							fromLevel = child.ToInt();

							break;
						case "Level":
							level = child.ToInt();

							break;
						case "MainPieces":
							foreach (var piece in child.Children)
							{
								importantParts.Add(piece.Key, piece.ToMPos());
							}

							break;
						case "Pieces":
							parts = child.ToArray();

							break;
						case "Exits":
							exits = child.ToArray();

							break;
						case "SpawnPoint":
							spawnPoint = child.ToMPos();

							break;
						case "Entrances":
							entrances = child.ToArray();

							break;
						case "Wall":
							wall = child.ToInt();

							break;
						case "BaseTerrainGeneration":
							baseterrain = TerrainGenerationType.GetType(0, child.Children.ToArray());

							break;
						case "TerrainGeneration":
							terrainGen.Add(TerrainGenerationType.GetType(child.ToInt(), child.Children.ToArray()));

							break;
						case "StructureGeneration":
							structureGen.Add(StructureGenerationType.GetType(child.ToInt(), child.Children.ToArray()));

							break;
						case "CustomSize":
							customSize = child.ToMPos();

							break;
						case "Ambient":
							ambient = child.ToColor();

							break;
					}
				}

				if (baseterrain == null)
					throw new YamlMissingNodeException(terrain.Key, "BaseTerrainGeneration");

				AddType(new MapType(parts, entrances, exits, importantParts, wall, customSize, ambient, playType, playModes, level, fromLevel, baseterrain, terrainGen.ToArray(), structureGen.ToArray(), spawnPoint), name);
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
				throw new MissingFieldException(string.Format("There are no Normal Maps."));

			return mainTypes[Program.SharedRandom.Next(mainTypes.Count())];
		}

		public static MapType FindTutorial()
		{
			var mainTypes = types.Values.Where(a => a.DefaultType == GameType.TUTORIAL).ToList();

			if (mainTypes.Count == 0)
				throw new MissingFieldException(string.Format("There are no Tutorial Maps."));

			return mainTypes[Program.SharedRandom.Next(mainTypes.Count())];
		}
	}
}
