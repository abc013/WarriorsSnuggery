using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public sealed class TerrainLayer : ITickRenderable, IDisposable
	{
		public Terrain[,] Terrain { get; private set; }
		List<Terrain> renderList = new List<Terrain>();
		bool listChanged = false;

		public TerrainLayer()
		{
			Terrain = new Terrain[0, 0];
		}

		public void SetMapDimensions(MPos size)
		{
			Dispose();
			Terrain = new Terrain[size.X, size.Y];
		}

		public void Set(Terrain terrain)
		{
			var position = terrain.Position;
			if (Terrain[position.X, position.Y] != null)
			{
				//Terrain[position.X, position.Y].Dispose();
				renderList.Remove(Terrain[position.X, position.Y]);
			}

			Terrain[position.X, position.Y] = terrain;
			renderList.Add(terrain);
			listChanged = true;
		}

		public void Tick()
		{
			foreach (var terrain in renderList)
				terrain.Tick();
		}

		public void Render()
		{
			if (listChanged)
				renderList = renderList.OrderBy(t => t.Type.OverlapHeight).ToList();
			listChanged = false;

			foreach (var terrain in renderList)
				terrain.Render();
		}

		public void Dispose()
		{
			renderList.Clear();
		}
	}
}
