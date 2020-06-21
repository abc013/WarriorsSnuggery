using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public sealed class TerrainLayer : ITickRenderable, IDisposable
	{
		public Terrain[,] Terrain { get; private set; }
		MPos size;

		public TerrainLayer()
		{
			Terrain = new Terrain[0, 0];
		}

		public void SetMapDimensions(MPos size)
		{
			Dispose();
			this.size = size;
			Terrain = new Terrain[size.X, size.Y];
		}

		public void Set(Terrain terrain)
		{
			var position = terrain.Position;

			Terrain[position.X, position.Y] = terrain;
		}

		public void Tick()
		{
			var bounds = VisibilitySolver.GetBounds(out var position);

			for (int x = position.X; x < position.X + bounds.X; x++)
			{
				if (x >= 0 && x < size.X)
				{
					for (int y = position.Y; y < position.Y + bounds.Y; y++)
					{
						if (y >= 0 && y < size.Y)
							Terrain[x, y].Tick();
					}
				}
			}
		}

		public void Render()
		{
			var bounds = VisibilitySolver.GetBounds(out var position);
			var renderList = new List<Terrain>();

			for (int x = position.X; x < position.X + bounds.X; x++)
			{
				if (x >= 0 && x < size.X)
				{
					for (int y = position.Y; y < position.Y + bounds.Y; y++)
					{
						if (y >= 0 && y < size.Y)
							renderList.Add(Terrain[x, y]);
					}
				}
			}
			var renderEnum = renderList.OrderBy(t => t.Type.OverlapHeight);

			foreach (var terrain in renderEnum)
				terrain.Render();
		}

		public void Dispose()
		{
			Terrain = new Terrain[0, 0];
		}
	}
}
