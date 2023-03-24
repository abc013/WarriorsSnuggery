using System.Collections.Generic;
using System.Linq;
using System.Text;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps.Layers
{
	public sealed class TerrainLayer : ISaveable
	{
		public static int TilesVisible { get; private set; }

		public readonly Terrain[,] Terrain;
		readonly MPos bounds;

		public TerrainLayer(MPos bounds)
		{
			Terrain = new Terrain[bounds.X, bounds.Y];
			this.bounds = bounds;
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

			TilesVisible = renderList.Count;

			foreach (var terrain in renderList.OrderBy(t => t.Type.OverlapHeight))
				terrain.Render();
		}

		public void CheckBordersAround(MPos pos)
		{
			for (int x = pos.X - 1; x < pos.X + 2; x++)
			{
				if (x >= 0 && x < bounds.X)
				{
					for (int y = pos.Y - 1; y < pos.Y + 2; y++)
					{
						if (y >= 0 && y < bounds.Y)
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

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			
			var builder = new StringBuilder(bounds.X * bounds.Y * 2, bounds.X * bounds.Y * 4);

			for (int y = 0; y < bounds.Y; y++)
				for (int x = 0; x < bounds.X; x++)
					builder.Append($"{Terrain[x, y].Type.ID},");

			builder.Remove(builder.Length - 1, 1);
			saver.Add("Terrain", builder);
			return saver;
		}
	}
}
