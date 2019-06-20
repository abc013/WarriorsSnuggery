using System;

namespace WarriorsSnuggery.Maps
{
	public class PathGenerationType
	{
		public readonly int ID;

		public readonly int Size;
		public readonly int[] Types;

		public readonly bool InGrid;
		public readonly MPos GridDimensions;

		public readonly bool FromCenter;
		public readonly int MaxCount;
		public readonly int MinCount;

		public readonly float Ruinous;
		public readonly float[] RuinousFallOff;

		PathGenerationType(int id, int size, int[] types, bool inGrid, MPos gridDimensions, bool fromCenter, int maxCount, int minCount, float ruinous, float[] ruinousFallOff)
		{
			ID = id;
			Size = size;
			Types = types;
			InGrid = inGrid;
			GridDimensions = gridDimensions;
			FromCenter = fromCenter;
			MaxCount = maxCount;
			MinCount = minCount;
			Ruinous = ruinous;
			RuinousFallOff = ruinousFallOff;
		}

		public static PathGenerationType GetType(int id, MiniTextNode[] nodes)
		{
			var size = 2;
			var types = new[] { 0 };
			var inGrid = false;
			var gridDimensions = MPos.Zero;
			var fromCenter = true;
			var maxCount = 5;
			var minCount = 1;
			var ruinous = 0f;
			var ruinousFallOff = new[] { 0f, 0f, 0f, 0.5f, 0.9f };

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Size":
						size = node.Convert<int>();

						break;
					case "Types":
						var rawType = node.Convert<string>().Split(',');
						types = new int[rawType.Length];
						for(int i = 0; i < types.Length; i++)
						{
							if (int.TryParse(rawType[i], out int val))
							{
								types[i] = val;
							}
							else
							{
								throw new YamlInvalidFormatException(node.Key, typeof(int[]));
							}
						}

						break;
					case "InGrid":
						inGrid = node.Convert<bool>();

						break;
					case "GridDimensions":
						gridDimensions = node.Convert<MPos>();

						break;
					case "FromCenter":
						fromCenter = node.Convert<bool>();

						break;
					case "MaxCount":
						maxCount = node.Convert<int>();

						break;
					case "MinCount":
						minCount = node.Convert<int>();

						break;
					case "Ruinous":
						ruinous = node.Convert<float>();

						break;
					case "RuinousFallOff":
						var rawFallOff = node.ToString().Split(',');
						ruinousFallOff = new float[rawFallOff.Length];
						for (int i = 0; i < types.Length; i++)
						{
							if (int.TryParse(rawFallOff[i], out int val))
							{
								types[i] = val;
							}
							else
							{
								throw new YamlInvalidFormatException(node.Key, typeof(float[]));
							}
						}

						break;
				}
			}

			return new PathGenerationType(id, size, types, inGrid, gridDimensions, fromCenter, maxCount, minCount, ruinous, ruinousFallOff);
		}
	}
}
