namespace WarriorsSnuggery.Maps
{
	public enum StructureGenerationMode
	{
		RANDOM,
		BLOCK,
		FLOCK,
		NONE
	}

	public sealed class StructureGenerationType
	{
		public readonly int ID;

		public readonly string[] Pieces;
		public readonly int[] SpawnsOn;
		public readonly int SpawnFrequency;
		public readonly StructureGenerationMode Mode;

		public readonly int MinimumSize;
		public readonly int MaximumSize;

		public readonly int Distance;

		public readonly bool Overrideable;

		StructureGenerationType(int id, string[] pieces, int[] spawnsOn, int spawnFrequency, StructureGenerationMode mode, int minimumSize, int maximumSize, int distance, bool overrideable)
		{
			ID = id;

			Pieces = pieces;
			SpawnsOn = spawnsOn;
			SpawnFrequency = spawnFrequency;
			Mode = mode;

			MinimumSize = minimumSize;
			MaximumSize = maximumSize;

			Distance = distance;

			Overrideable = overrideable;
		}

		public static StructureGenerationType Empty()
		{
			return new StructureGenerationType(0, new string[0], new int[0], 0, StructureGenerationMode.NONE, 0, 0, 0, true);
		}

		public static StructureGenerationType GetType(int id, MiniTextNode[] nodes)
		{
			var pieces = new string[0];
			var spawnsOn = new int[0];
			var spawnFrequency = 100;
			var generationMode = StructureGenerationMode.NONE;

			var minimumSize = 0;
			var maximumSize = 10;

			var distance = 2;

			var overrideable = false;

			foreach(var node in nodes)
			{
				switch(node.Key)
				{
					case "Pieces":
						pieces = node.ToArray();

						break;
					case "SpawnsOn":
						var spawnsOnStrings = node.ToArray();

						spawnsOn = new int[spawnsOnStrings.Length];
						for(int i = 0; i < spawnsOn.Length; i++)
						{
							spawnsOn[i] = int.Parse(spawnsOnStrings[i]);
						}

						break;
					case "SpawnFrequency":
						spawnFrequency = node.ToInt();

						break;
					case "GenerationMode":
						generationMode = (StructureGenerationMode) node.ToEnum(typeof(StructureGenerationMode));

						break;
					case "MinimumSize":
						minimumSize = node.ToInt();

						break;
					case "MaximumSize":
						maximumSize = node.ToInt();

						break;
					case "Distance":
						distance = node.ToInt();

						break;
					case "Overrideable":
						overrideable = node.ToBoolean();

						break;

				}
			}

			return new StructureGenerationType(id, pieces, spawnsOn, spawnFrequency, generationMode, minimumSize, maximumSize, distance, overrideable);
		}
	}
}
