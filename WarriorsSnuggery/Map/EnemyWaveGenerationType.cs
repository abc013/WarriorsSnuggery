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
						types = node.ToArray();

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

			return new EnemyWaveGenerationType(id, types, spawnProbability, maximumWaves, difficulty, spawnsOn);
		}
	}
}
