using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps
{
	public class TerrainGenerationType
	{
		public readonly NoiseType Noise;
		public readonly int Strength;
		public readonly float Intensity;
		public readonly float Contrast;
		public readonly int[] Terrain;
		public readonly bool SpawnPieces;
		public readonly int[] BorderTerrain;
		public readonly int Border;
		public readonly Dictionary<ActorType, int> SpawnActors;

		public TerrainGenerationType(NoiseType noise, int strength, float intensity, float contrast, int[] terrain, bool spawnPieces, int[] borderTerrain, int border, Dictionary<ActorType, int> spawnActors)
		{
			Noise = noise;
			Strength = strength;
			Intensity = intensity;
			Contrast = contrast;
			Terrain = terrain;
			SpawnPieces = spawnPieces;
			BorderTerrain = borderTerrain;
			Border = border;
			SpawnActors = spawnActors;
		}
	}
}
