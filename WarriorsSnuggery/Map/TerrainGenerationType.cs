using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps
{
	public sealed class TerrainGenerationType
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

		TerrainGenerationType(int id, GenerationType generationType, int strength, float scale, float intensity, float contrast, int[] terrain, bool spawnPieces, int[] borderTerrain, int border, Dictionary<ActorType, int> spawnActors)
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

		public static TerrainGenerationType Empty()
		{
			return new TerrainGenerationType(0, GenerationType.NONE, 1, 1f, 1f, 1f, new[] { 0 }, true, new int[] { }, 0, new Dictionary<ActorType, int>());
		}

		public static TerrainGenerationType GetType(int id, MiniTextNode[] nodes)
		{
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

			foreach (var generation in nodes)
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
			return new TerrainGenerationType(id, noise, strength, scale, intensity, contrast, terrainTypes, spawnPieces, borderTerrain, border, spawnActorBlob);
		}
	}
}