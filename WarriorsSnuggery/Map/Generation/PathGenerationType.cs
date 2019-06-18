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

		PathGenerationType()
		{

		}

		public static PathGenerationType GetType(int id, MiniTextNode[] nodes)
		{
			//var size = 2;
			//var types = new[] { 0 };
			//var inGrid = false;
			//var gridDimensions = MPos.Zero;
			//var fromCenter = true;
			//var maxCount = 5;
			//var minCount = 1;
			//var ruinous = 0f;
			//var ruinousFallOff = new[] { 0f, 0f, 0f, 0.5f, 0.9f };

			//foreach(var node in nodes)
			//{
			//	switch(node.Key)
			//	{
			//		case "Size":
			//			size = node.ToInt();

			//			break;
			//		case "Types":
			//			var rawType = node.ToString().Split(',')

			//			break;
			//		case "InGrid":

			//			break;
			//		case "GridDimensions":

			//			break;
			//		case "FromCenter":

			//			break;
			//		case "MaxCount":

			//			break;
			//		case "MinCount":

			//			break;
			//		case "Ruinous":

			//			break;
			//		case "RuinousFallOff":

			//			break;
			//	}
			//}

			return new PathGenerationType();
		}
	}
}
