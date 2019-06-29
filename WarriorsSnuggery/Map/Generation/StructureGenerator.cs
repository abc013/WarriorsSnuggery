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
						pieces = node.Convert<string[]>();

						break;
					case "SpawnsOn":
						spawnsOn = node.Convert<int[]>();

						break;
					case "SpawnFrequency":
						spawnFrequency = node.Convert<int>();

						break;
					case "GenerationMode":
						generationMode = node.Convert<StructureGenerationMode>();

						break;
					case "MinimumSize":
						minimumSize = node.Convert<int>();

						break;
					case "MaximumSize":
						maximumSize = node.Convert<int>();

						break;
					case "Distance":
						distance = node.Convert<int>();

						break;
					case "Overrideable":
						overrideable = node.Convert<bool>();

						break;

				}
			}

			return new StructureGenerationType(id, pieces, spawnsOn, spawnFrequency, generationMode, minimumSize, maximumSize, distance, overrideable);
		}
	}
}
