using System;

namespace WarriorsSnuggery.Maps
{
	public enum StructureGenerationMode
	{
		RANDOM,
		BLOCK,
		FLOCK,
		NONE
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

		public static StructureGenerationType GetType(int id, MiniTextNode[] nodes)
		{
			var pieces = new string[0];
			var spawnsOn = new int[0];
			var generationMode = StructureGenerationMode.NONE;

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
					case "GenerationMode":
						generationMode = (StructureGenerationMode) node.ToEnum(typeof(StructureGenerationMode));

						break;
				}
			}

			return new StructureGenerationType(id, pieces, spawnsOn, generationMode);
		}
	}
}
