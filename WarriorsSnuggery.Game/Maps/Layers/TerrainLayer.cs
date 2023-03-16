using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps.Layers
{
	public sealed class TerrainLayer : ITickRenderable
	{
		public readonly Terrain[,] Terrain;

		public TerrainLayer(MPos bounds)
		{
			Terrain = new Terrain[bounds.X, bounds.Y];
		}

		public void Set(Terrain terrain)
		{
			var position = terrain.Position;

			Terrain[position.X, position.Y] = terrain;
		}

		public void Tick()
		{
			CameraVisibility.GetClampedBounds(out var position, out var bounds);

			for (int x = position.X; x < position.X + bounds.X; x++)
			{
				for (int y = position.Y; y < position.Y + bounds.Y; y++)
					Terrain[x, y].Tick();
			}
		}

		public void Render()
		{
			CameraVisibility.GetClampedBounds(out var position, out var bounds);

			var renderList = new List<Terrain>();

			for (int x = position.X; x < position.X + bounds.X; x++)
			{
				for (int y = position.Y; y < position.Y + bounds.Y; y++)
					renderList.Add(Terrain[x, y]);
			}

			foreach (var terrain in renderList.OrderBy(t => t.Type.OverlapHeight))
				terrain.Render();
		}

		public void CheckBordersAround(MPos pos)
		{
			for (int x = pos.X - 1; x < pos.X + 2; x++)
			{
				if (x >= 0 && x < Terrain.GetLength(0))
				{
					for (int y = pos.Y - 1; y < pos.Y + 2; y++)
					{
						if (y >= 0 && y < Terrain.GetLength(1))
						{
							Terrain[x, y].CheckEdgeVisibility();
						}
					}
				}
			}
		}

		public void CheckBorders()
		{
			foreach (var terrain in Terrain)
				terrain.CheckEdgeVisibility();
		}
	}
}
