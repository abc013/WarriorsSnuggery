using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Maps
{
	public class GridGenerator : MapGenerator
	{
		readonly GridGeneratorInfo type;

		readonly float[,] noise;

		readonly List<MPos> points = new List<MPos>();

		public GridGenerator(Random random, Map map, World world, GridGeneratorInfo type) : base(random, map, world)
		{
			this.type = type;

			noise = new float[map.Bounds.X, map.Bounds.Y];
		}

		public override void Generate()
		{
			throw new NotImplementedException();
		}

		protected override void MarkDirty()
		{
			throw new NotImplementedException();
		}

		protected override void DrawDirty()
		{
			throw new NotImplementedException();
		}
	}

	[Desc("Generator used for generating grid-based towns or structures.")]
	public sealed class GridGeneratorInfo
	{
		[Desc("Unique ID for the generator.")]
		public readonly int ID;

		[Desc("Size of the Grid.", "Must be a multiplex of RoadSize.")]
		public readonly int GridSize = 4;
		[Desc("Size of the Roads inbetween the grid.")]
		public readonly int RoadSize = 2;
		[Desc("Decides wether the grid is aligned rectangular or has random roads.")]
		public readonly bool Rectangular = true;

		[Desc("Defines to what percentage the road should be overgrown.")]
		public readonly float RuinousRoad = 0.1f;
		[Desc("Defines to what percentage the road should be overgrown.", "The first value will be used in the mid, the last at the edges of the map. Those in between will be used on linear scale.")]
		public readonly float[] RuinousRoadFalloff = new[] { 0f, 0f, 0.1f, 0.2f, 0.3f, 0.8f };

		public GridGeneratorInfo(int id, MiniTextNode[] nodes)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);
		}
	}
}
