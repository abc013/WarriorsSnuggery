﻿using System;
using System.Linq;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public class TerrainLayer : IRenderable, IDisposable
	{
		public Terrain[,] Terrain { get; private set; }
		List<Terrain> renderList = new List<Terrain>();
		bool listChanged = false;
		public MPos Size { get; private set; }

		public TerrainLayer()
		{
			Terrain = new Terrain[0,0];
			Size = MPos.Zero;
		}

		public void SetMapSize(MPos size)
		{
			Dispose();
			Size = size;
			Terrain = new Terrain[size.X,size.Y];
		}

		public void Set(Terrain terrain)
		{
			var position = terrain.Position;
			if (Terrain[position.X, position.Y] != null)
				renderList.Remove(Terrain[position.X, position.Y]);

			Terrain[position.X, position.Y] = terrain;
			renderList.Add(terrain);
			listChanged = true;
		}

		public void Render()
		{
			if (listChanged)
				renderList = renderList.OrderBy(t => t.Type.OverlapHeight).ToList();
			listChanged = false;

			foreach(var terrain in renderList)
				terrain.Render();
		}

		public void Dispose()
		{
			renderList.Clear();
		}
	}
}
