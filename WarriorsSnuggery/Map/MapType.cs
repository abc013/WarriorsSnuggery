using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps
{
	public class MapType
	{
		public readonly int Level;
		public readonly int FromLevel;
		public readonly GameType DefaultType;
		public readonly GameMode DefaultMode;
		public readonly string[] Parts;
		public readonly string[] Entrances;
		public readonly string[] Exits;
		public readonly Dictionary<string, MPos> ImportantParts = new Dictionary<string, MPos>();
		public readonly TerrainGenerationType[] TerrainGeneration;
		public readonly TerrainGenerationType BaseTerrainGeneration;
		public readonly int Wall;
		public readonly MPos CustomSize;
		public readonly Color Ambient;
		public readonly Dictionary<ActorType, int[]> SpawnActors;
		public readonly MPos SpawnPoint;

		public MapType(string[] parts, string[] entrances, string[] exits, Dictionary<string, MPos> importantParts, int wall, MPos customSize, Color ambient, GameType defaultType, GameMode defaultMode, int level, int fromLevel, Dictionary<ActorType, int[]> spawnActors, TerrainGenerationType baseTerrainGeneration, TerrainGenerationType[] terrainGeneration, MPos spawnPoint)
		{
			DefaultType = defaultType;
			DefaultMode = defaultMode;
			Level = level;
			FromLevel = fromLevel;
			Parts = parts;
			Entrances = entrances;
			Exits = exits;
			ImportantParts = importantParts;
			Wall = wall;
			CustomSize = customSize;
			Ambient = ambient;
			SpawnActors = spawnActors;
			BaseTerrainGeneration = baseTerrainGeneration;
			TerrainGeneration = terrainGeneration;
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
			var gen = new TerrainGenerationType(0, GenerationType.NONE, 2, 1f, 1f, 1f, new[] { 0 }, true, new int[] { }, 0, new Dictionary<ActorType, int>());
			return new MapType(new string[] { }, new string[] { }, new string[] { }, dict, 0, size, Color.White, GameType.EDITOR, GameMode.NONE, -1, 0, new Dictionary<ActorType, int[]>(), gen, new TerrainGenerationType[0], MPos.Zero);
		}

		public static MapType ConvertGameType(MapType map, GameType type)
		{
			return new MapType(map.Parts, map.Entrances, map.Exits, map.ImportantParts, map.Wall, map.CustomSize, map.Ambient, type, map.DefaultMode, map.Level, map.FromLevel, map.SpawnActors, map.BaseTerrainGeneration, map.TerrainGeneration, map.SpawnPoint);
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
				// TODO: playmode possibilites here, take a random at game then;
				var playMode = GameMode.NONE;
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
				var spawnActors = new Dictionary<ActorType, int[]>();
				var terrainGen = new List<TerrainGenerationType>();
				var spawnPoint = new MPos(-1, -1);
				TerrainGenerationType baseterrain = null;

				foreach (var child in terrain.Children)
				{
					switch (child.Key)
					{
						case "PlayType":
							playType = (GameType)child.ToEnum(typeof(GameType));

							break;
						case "ActiveFromLevel":
							fromLevel = child.ToInt();

							break;
						case "Level":
							level = child.ToInt();

							break;
						case "SpawnActor":
							if (child.Children.Count != 0)
							{
								int[] tiles = null;
								foreach (var ground in child.Children)
								{
									var array = ground.ToArray();
									tiles = new int[array.Length];
									for (int i = 0; i < array.Length; i++)
									{
										int.TryParse(array[i], out tiles[i]);
									}
								}
								spawnActors.Add(ActorCreator.GetType(child.Value), tiles);
							}
							else
							{
								spawnActors.Add(ActorCreator.GetType(child.Value), null);
							}

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
						case "TerrainGeneration":
							var id = child.ToInt();
							var noise = GenerationType.NONE;
							var strength = 8;
							var scale = 2f;
							var intensity = 0f;
							var contrast = 1f;
							var terrainTypes = new int[0];
							var spawnPieces = true;
							var borderTerrain = new int[0];
							var border = 0;
							var spawnActorBlob = new Dictionary<ActorType, int>();

							foreach (var generation in child.Children)
							{
								switch (generation.Key)
								{
									case "Type":
										noise = (GenerationType)generation.ToEnum(typeof(GenerationType));

										foreach (var noiseChild in generation.Children)
										{
											switch (noiseChild.Key)
											{
												case "Strength":
													strength = noiseChild.ToInt();
													break;
												case "Scale":
													scale = noiseChild.ToFloat();
													break;
												case "Contrast":
													contrast = noiseChild.ToFloat();
													break;
												case "Intensity":
													intensity = noiseChild.ToFloat();
													break;
											}
										}
										break;
									case "Terrain":
										var rawTerrain = generation.ToArray();
										terrainTypes = new int[rawTerrain.Length];

										for (int i = 0; i < rawTerrain.Length; i++)
											terrainTypes[i] = int.Parse(rawTerrain[i]);

										break;
									case "Border":
										border = generation.ToInt();

										var rawBorder = generation.Children.FindAll(n => n.Key == "Terrain").ToArray();
										borderTerrain = new int[rawBorder.Length];

										for (int i = 0; i < rawBorder.Length; i++)
											borderTerrain[i] = int.Parse(rawBorder[i].Value);

										break;
									case "SpawnPieces":
										spawnPieces = generation.ToBoolean();

										break;
									case "SpawnActor":
										var type = ActorCreator.GetType(generation.Value);
										var probability = 50;

										probability = generation.Children.Find(n => n.Key == "Probability").ToInt();

										spawnActorBlob.Add(type, probability);
										break;
								}
							}

							if (child.Key == "TerrainGeneration")
								terrainGen.Add(new TerrainGenerationType(id, noise, strength, scale, intensity, contrast, terrainTypes, spawnPieces, borderTerrain, border, spawnActorBlob));
							else
								baseterrain = new TerrainGenerationType(0, noise, strength, scale, intensity, contrast, terrainTypes, spawnPieces, borderTerrain, border, spawnActorBlob);
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

				AddType(new MapType(parts, entrances, exits, importantParts, wall, customSize, ambient, playType, playMode, level, fromLevel, spawnActors, baseterrain, terrainGen.ToArray(), spawnPoint), name);
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
