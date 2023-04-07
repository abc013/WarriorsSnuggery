using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps.Generators;
using WarriorsSnuggery.Maps.Noises;
using WarriorsSnuggery.Maps.Pieces;
using WarriorsSnuggery.Objectives;
using WarriorsSnuggery.Objects.Weather;

namespace WarriorsSnuggery.Maps
{
	[Desc("These rules contain information about how to generate a map.", "Apart from static attributes, there are also NoiseMaps and Generators. Those are used to generate unique maps for every level and seed.")]
	public class MapType
	{
		public readonly string Name;
		public readonly bool IsSave;

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
		public readonly PackageFile OverridePiece;
		[Desc("Custom size for the map.")]
		public readonly MPos CustomSize = MPos.Zero;
		[Desc("Spawn point of the player.", "If left at (0, 0, 0), the center of the map or a generated spawnpoint will be used.")]
		public readonly CPos SpawnPoint = CPos.Zero;
		[Desc("Allows the use of weapons on the map.")]
		public readonly bool AllowWeapons = true;
		[Desc("Reveals the whole map.")]
		public readonly bool RevealMap = false;

		[Desc("Ambient color of the map.", "Note: lowering this factor to below a quarter of strength will get weird results with lights.")]
		public readonly Color Ambient = Color.White;
		[Desc("Varying ambient color of the map that will be randomized and added upon the normal Ambient.", "Note: lowering this factor to below a quarter of strength will get weird results with lights.")]
		public readonly float AmbientVariance = 0f;
		[Desc("Varying ambient color of the map that will be randomized per-channel and added upon the normal Ambient.", "Note: lowering this factor to below a quarter of strength will get weird results with lights.")]
		public readonly Color AmbientColorVariance = Color.Black;
		[Desc("Wall type to use when surrounding the map with walls.")]
		public readonly int Wall = 0;
		[Desc("Amount of terrain cells to use for a world border.", "This border can't be accessed and fades out into black visually.")]
		public readonly int WorldBorder = 0;

		[Desc("Generators that determine how the map should look like", "The generators are used in the order in which they are written, which means from top to bottom.")]
		public readonly IMapGeneratorInfo[] Generators = new IMapGeneratorInfo[0];

		[Desc("Noises that are referenced by the generators.")]
		public readonly NoiseMapInfo[] NoiseMaps = new NoiseMapInfo[0];

		[Desc("Patrol generators used for generating enemies and waves.")]
		public readonly PatrolPlacerInfo[] PatrolPlacers = new PatrolPlacerInfo[0];

		[Desc("Weather effects to use on this map.")]
		public readonly WeatherEffect[] WeatherEffects = new WeatherEffect[0];

		[Desc("Determines the file of a script that will be executed during the game.", "Ending of the filename must be '.cs'.")]
		public readonly PackageFile MissionScript;

		[Desc("Song looping while being in the level.", "If left empty, music will cycle.")]
		public readonly PackageFile Music;

		[Desc("Battle song looping while being low life or in combat.", "If left empty, there will be no battle music and normal music keeps playing.", "Warning: do not choose the same song as battle song again. This will crash the game.")]
		public readonly PackageFile IntenseMusic;

		// For the DocWriter
		public MapType() { }

		MapType(string name, List<TextNode> nodes)
		{
			Name = name;

			var fields = TypeLoader.GetFields(this);

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
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
					case nameof(WeatherEffects):
						WeatherEffects = new WeatherEffect[node.Children.Count];

						for (int i = 0; i < WeatherEffects.Length; i++)
							WeatherEffects[i] = new WeatherEffect(node.Children[i].Children);

						break;
					default:
						TypeLoader.SetValue(this, fields, node);

						break;
				}
			}
		}

		MapType(PackageFile overridePiece, int wall, int worldBorder, MPos customSize, Color ambient, MissionType[] missionTypes, ObjectiveType[] availableObjectives, int level, int fromLevel, int toLevel, IMapGeneratorInfo[] generators, CPos spawnPoint, bool isSave, bool revealMap, bool allowWeapons, PackageFile missionScript, WeatherEffect[] weatherEffects, PackageFile music, PackageFile intenseMusic)
		{
			OverridePiece = overridePiece;
			Wall = wall;
			WorldBorder = worldBorder;
			CustomSize = customSize;
			Ambient = ambient;
			MissionTypes = missionTypes;
			AvailableObjectives = availableObjectives;
			Level = level;
			FromLevel = fromLevel;
			ToLevel = toLevel;
			Generators = generators;
			SpawnPoint = spawnPoint;
			IsSave = isSave;
			RevealMap = revealMap;
			AllowWeapons = allowWeapons;
			MissionScript = missionScript;
			WeatherEffects = weatherEffects;
			Music = music;
			IntenseMusic = intenseMusic;
		}

		public Color GetAmbience(Random random)
		{
			var variance = (float)(random.NextDouble() * AmbientVariance) * Color.White.WithAlpha(0f);
			var colorVariance = new Color(AmbientColorVariance.R * (float)random.NextDouble(), AmbientColorVariance.G * (float)random.NextDouble(), AmbientColorVariance.B * (float)random.NextDouble(), 0f);

			return Ambient + variance + colorVariance;
		}

		public static MapType FromRules(TextNode parent)
		{
			return new MapType(parent.Key, parent.Children);
		}

		public static MapType FromSave(GameSave save)
		{
			var size = TextNodeLoader.FromFile(FileExplorer.Saves, save.MapSaveName + ".yaml").First(n => n.Key == "Size").Convert<MPos>();
			var type = save.CurrentMapType;

			return new MapType(new PackageFile(save.MapSaveName), type.Wall, type.WorldBorder, size, save.CurrentAmbience, new[] { save.CurrentMission }, new[] { save.CurrentObjective }, -1, 0, int.MaxValue, type.Generators, CPos.Zero, true, type.RevealMap, type.AllowWeapons, save.Script, type.WeatherEffects, type.Music, type.IntenseMusic);
		}

		public static MapType FromPiece(Piece piece, MissionType type = MissionType.TEST, ObjectiveType objective = ObjectiveType.NONE)
		{
			return new MapType(piece.PackageFile, 0, 0, piece.Size, Color.White, new[] { type }, new[] { objective }, -1, 0, int.MaxValue, new IMapGeneratorInfo[0], CPos.Zero, false, true, true, null, new WeatherEffect[0], null, null);
		}
	}
}
