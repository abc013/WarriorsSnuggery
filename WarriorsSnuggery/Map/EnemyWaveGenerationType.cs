namespace WarriorsSnuggery.Maps
{
	public sealed class EnemyWaveGenerationType
	{
		public readonly int ID;

		public readonly string[] Types;
		public readonly int[] Counts;

		public readonly float SpawnProbability;
		public readonly int MaximumWaves;
		public readonly int Difficulty;

		public readonly int[] SpawnsOn;

		EnemyWaveGenerationType(int id, string[] types, int[] counts, float spawnProbability, int maximumWaves, int difficulty, int[] spawnsOn)
		{
			ID = id;

			Types = types;
			Counts = counts;
			SpawnProbability = spawnProbability;
			MaximumWaves = maximumWaves;
			Difficulty = difficulty;

			SpawnsOn = spawnsOn;
		}

		public static EnemyWaveGenerationType GetType(int id, MiniTextNode[] nodes)
		{
			var types = new string[0];
			var counts = new int[0];

			var spawnProbability = 1f;
			var maximumWaves = 2;
			var difficulty = 1;

			var spawnsOn = new int[0];

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Types":
						types = node.ToArray();

						break;
					case "Counts":
						var countStrings = node.ToArray();

						counts = new int[countStrings.Length];
						for (int i = 0; i < counts.Length; i++)
						{
							counts[i] = int.Parse(countStrings[i]);
						}

						break;
					case "Probability":
						spawnProbability = node.ToFloat();

						break;
					case "MaximumWaves":
						maximumWaves = node.ToInt();

						break;
					case "Difficulty":
						difficulty = node.ToInt();

						break;
					case "SpawnsOn":
						var spawnsOnStrings = node.ToArray();

						spawnsOn = new int[spawnsOnStrings.Length];
						for (int i = 0; i < spawnsOn.Length; i++)
						{
							spawnsOn[i] = int.Parse(spawnsOnStrings[i]);
						}


						break;

				}
			}

			return new EnemyWaveGenerationType(id, types, counts, spawnProbability, maximumWaves, difficulty, spawnsOn);
		}
	}
}
