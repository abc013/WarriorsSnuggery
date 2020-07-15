using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public sealed class TerrainLayer : ITickRenderable
	{
		public Terrain[,] Terrain { get; private set; }
		readonly MPos bounds;

		public TerrainLayer(MPos bounds)
		{
			this.bounds = bounds;
			Terrain = new Terrain[bounds.X, bounds.Y];
		}

		public void Set(Terrain terrain)
		{
			var position = terrain.Position;

			Terrain[position.X, position.Y] = terrain;
		}

		public void Tick()
		{
			var visibilityBounds = VisibilitySolver.GetBounds(out var position);

			for (int x = position.X; x < position.X + visibilityBounds.X; x++)
			{
				if (x >= 0 && x < bounds.X)
				{
					for (int y = position.Y; y < position.Y + visibilityBounds.Y; y++)
					{
						if (y >= 0 && y < bounds.Y)
							Terrain[x, y].Tick();
					}
				}
			}
		}

		public void Render()
		{
			var visibilityBounds = VisibilitySolver.GetBounds(out var position);
			var renderList = new List<Terrain>();

			for (int x = position.X; x < position.X + visibilityBounds.X; x++)
			{
				if (x >= 0 && x < bounds.X)
				{
					for (int y = position.Y; y < position.Y + visibilityBounds.Y; y++)
					{
						if (y >= 0 && y < bounds.Y)
							renderList.Add(Terrain[x, y]);
					}
				}
			}
			var renderEnum = renderList.OrderBy(t => t.Type.OverlapHeight);

			foreach (var terrain in renderEnum)
				terrain.Render();
		}

		public void Clear()
		{
			Terrain = new Terrain[0, 0];
		}
	}
}
