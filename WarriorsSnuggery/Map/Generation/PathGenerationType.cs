using System;

namespace WarriorsSnuggery.Maps
{
	public class PathGenerationType
	{
		public readonly int ID;

		public readonly int Size;
		public readonly int[] Types;

		public readonly bool FromEntrance;
		public readonly bool ToExit;
		public readonly int MaxCount;
		public readonly int MinCount;

		public readonly float Ruinous;
		public readonly float[] RuinousFalloff;

		public readonly bool Curvy;

		PathGenerationType(int id, int size, int[] types, bool fromCenter, bool toExit, int maxCount, int minCount, float ruinous, float[] ruinousFallOff, bool curvy)
		{
			ID = id;
			Size = size;
			Types = types;
			FromEntrance = fromCenter;
			ToExit = toExit;
			MaxCount = maxCount;
			MinCount = minCount;
			Ruinous = ruinous;
			RuinousFalloff = ruinousFallOff;
			Curvy = curvy;
		}

		public static PathGenerationType GetType(int id, MiniTextNode[] nodes)
		{
			var size = 2;
			var types = new[] { 0 };
			var fromCenter = true;
			var toExit = true;
			var maxCount = 5;
			var minCount = 1;
			var ruinous = 0f;
			var ruinousFalloff = new[] { 0f, 0f, 0.1f, 0.2f, 0.3f, 0.8f };
			var curvy = true;

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Size":
						size = node.Convert<int>();

						break;
					case "Types":
						types = node.Convert<int[]>();

						break;
					case "FromEntrance":
						fromCenter = node.Convert<bool>();

						break;
					case "ToExit":
						toExit = node.Convert<bool>();

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
					case "RuinousFalloff":
						ruinousFalloff = node.Convert<float[]>();

						break;
					case "Curviness":
						curvy = node.Convert<bool>();

						break;
				}
			}

			return new PathGenerationType(id, size, types, fromCenter, toExit, maxCount, minCount, ruinous, ruinousFalloff, curvy);
		}
	}
}
