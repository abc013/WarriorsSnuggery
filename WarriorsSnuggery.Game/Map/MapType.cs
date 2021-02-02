using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps.Generators;

namespace WarriorsSnuggery.Maps
{
	[Desc("These rules contain information about how to generate a map.", "Apart from static attributes, there are also NoiseMaps and Generators. Those are used to generate unique maps for every level and seed.")]
	public class MapType
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

		[Desc("Terrain Generator as basis. Required for the game to function. This is the first generator to be used.")]
		public readonly TerrainGeneratorInfo TerrainGenerationBase = null;
		[Desc("Generators that determine how the map should look like", "The generators are used in the order in which they are written, which means from top to bottom.")]
		public readonly IMapGeneratorInfo[] Generators = new IMapGeneratorInfo[0];

		[Desc("Noises that are referenced by the generators.")]
		public readonly NoiseMapInfo[] NoiseMaps = new NoiseMapInfo[0];

		[Desc("Patrol generators used for generating enemies and waves.")]
		public readonly PatrolPlacerInfo[] PatrolPlacers = new PatrolPlacerInfo[0];

		[Desc("Determines the file of a script that will be executed during the game.", "Ending of the filename must be '.cs'.")]
		public readonly string MissionScript;

		[Desc("Variable used to determine wether this map comes from an save. DO NOT ALTER.")]
		public readonly bool IsSave;

		// For the DocWriter
		public MapType() { }

		MapType(string name, List<MiniTextNode> nodes)
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
					case nameof(Generators):
						Generators = new IMapGeneratorInfo[node.Children.Count];

						for (int i = 0; i < Generators.Length; i++)
						{
							var child = node.Children[i];
							Generators[i] = GeneratorLoader.GetGenerator(child.Key, child.Convert<int>(), child.Children);
						}

						break;
					case nameof(NoiseMaps):
						NoiseMaps = new NoiseMapInfo[node.Children.Count];

						for (int i = 0; i < NoiseMaps.Length; i++)
						{
							var child = node.Children[i];
							NoiseMaps[i] = new NoiseMapInfo(child.Convert<int>(), child.Children);
						}

						break;
					case nameof(PatrolPlacers):
						PatrolPlacers = new PatrolPlacerInfo[node.Children.Count];

						for (int i = 0; i < PatrolPlacers.Length; i++)
							PatrolPlacers[i] = new PatrolPlacerInfo(node.Children[i].Children);

						break;
					default:
						PartLoader.SetValue(this, fields, node);

						break;
				}
			}

			if (TerrainGenerationBase == null)
				throw new MissingNodeException(name, "BaseTerrainGeneration");
		}

		MapType(string overridePiece, int wall, MPos customSize, Color ambient, MissionType[] missionTypes, ObjectiveType[] availableObjectives, int level, int fromLevel, int toLevel, TerrainGeneratorInfo baseTerrainGeneration, IMapGeneratorInfo[] generators, MPos spawnPoint, bool isSave, bool allowWeapons, string missionScript)
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
			Generators = generators;
			SpawnPoint = spawnPoint;
			IsSave = isSave;
			AllowWeapons = allowWeapons;
			MissionScript = missionScript;
		}

		public static MapType FromRules(MiniTextNode parent)
		{
			return new MapType(parent.Key, parent.Children);
		}

		public static MapType FromSave(GameStatistics stats)
		{
			var size = RuleReader.FromFile(FileExplorer.Saves, stats.MapSaveName + ".yaml").First(n => n.Key == "Size").Convert<MPos>();

			var type = MapCreator.GetType(stats.CurrentMapType);
			var mapGeneratorInfos = type == null ? new IMapGeneratorInfo[0] : type.Generators;

			return new MapType(stats.MapSaveName, 0, size, Color.White, new[] { stats.CurrentMission }, new[] { stats.CurrentObjective }, -1, 0, int.MaxValue, new TerrainGeneratorInfo(0, new List<MiniTextNode>()), mapGeneratorInfos, MPos.Zero, true, true, stats.Script);
		}

		public static MapType FromPiece(Piece piece, MissionType type = MissionType.TEST, ObjectiveType objective = ObjectiveType.NONE)
		{
			return new MapType(piece.InnerName, 0, piece.Size, Color.White, new[] { type }, new[] { objective }, -1, 0, int.MaxValue, new TerrainGeneratorInfo(0, new List<MiniTextNode>()), new IMapGeneratorInfo[0], MPos.Zero, false, true, null);
		}
	}
}
