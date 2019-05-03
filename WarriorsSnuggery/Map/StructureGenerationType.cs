using System;

namespace WarriorsSnuggery.Maps
{
	public enum StructureGenerationMode
	{
		RANDOM,
		BLOCK,
		FLOCK
	}

	public class StructureGenerationType
	{
		public readonly int ID;

		public readonly string[] Pieces;
		public readonly int[] SpawnsOn;
		public readonly StructureGenerationMode Mode;

		public StructureGenerationType(int id, string[] pieces, int[] spawnsOn, StructureGenerationMode mode)
		{
			ID = id;

			Pieces = pieces;
			SpawnsOn = spawnsOn;
			Mode = mode;
		}

		public static void GetType(MiniTextNode[] nodes)
		{
			// TODO USE
		}
	}
}
