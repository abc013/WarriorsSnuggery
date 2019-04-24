using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps
{
	public class TerrainGenerationType
	{
		public readonly int ID;

		public readonly GenerationType GenerationType;
		public readonly int Strength;
		public readonly float Scale;

		public readonly float Intensity;
		public readonly float Contrast;

		public readonly int[] Terrain;
		public readonly bool SpawnPieces;
		public readonly int[] BorderTerrain;
		public readonly int Border;
		public readonly Dictionary<ActorType, int> SpawnActors;

		public TerrainGenerationType(int id, GenerationType generationType, int strength, float scale, float intensity, float contrast, int[] terrain, bool spawnPieces, int[] borderTerrain, int border, Dictionary<ActorType, int> spawnActors)
		{
			ID = id;
			GenerationType = generationType;
			Strength = strength;
			Scale = scale;
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
