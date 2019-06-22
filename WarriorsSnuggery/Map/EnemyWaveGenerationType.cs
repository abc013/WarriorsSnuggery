namespace WarriorsSnuggery.Maps
{
	public sealed class EnemyWaveGenerationType
	{
		public readonly int ID;

		public readonly string[] Types;

		public readonly float SpawnProbability;
		public readonly int MaximumWaves;
		public readonly int Difficulty;

		public readonly int[] SpawnsOn;

		EnemyWaveGenerationType(int id, string[] types, float spawnProbability, int maximumWaves, int difficulty, int[] spawnsOn)
		{
			ID = id;

			Types = types;
			SpawnProbability = spawnProbability;
			MaximumWaves = maximumWaves;
			Difficulty = difficulty;

			SpawnsOn = spawnsOn;
		}

		public static EnemyWaveGenerationType GetType(int id, MiniTextNode[] nodes)
		{
			var types = new string[0];

			var spawnProbability = 1f;
			var maximumWaves = 2;
			var difficulty = 1;

			var spawnsOn = new int[0];

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Types":
						types = node.Convert<string[]>();

						break;
					case "Probability":
						spawnProbability = node.Convert<float>();

						break;
					case "MaximumWaves":
						maximumWaves = node.Convert<int>();

						break;
					case "Difficulty":
						difficulty = node.Convert<int>();

						break;
					case "SpawnsOn":
						spawnsOn = node.Convert<int[]>();

						break;

				}
			}

			return new EnemyWaveGenerationType(id, types, spawnProbability, maximumWaves, difficulty, spawnsOn);
		}
	}
}
