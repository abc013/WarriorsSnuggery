/*
 * User: Andreas
 * Date: 15.07.2018
 * Time: 17:51
 * Croatia is loosing =( 4:1 for the frenchies wait the shot a goal 4:2 !!! Damn they lost :(((( but still, the first two goals of the french where just sh**. nevermind.
 */
using System;
using System.Linq;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public sealed class TerrainLayer : IRenderable, IDisposable
	{
		public Terrain[,] Terrain { get; private set; }
		List<Terrain> renderList = new List<Terrain>();
		bool listChanged = false;
		MPos size;

		public TerrainLayer()
		{
			Terrain = new Terrain[0,0];
			size = MPos.Zero;
		}

		public void SetMapSize(MPos size)
		{
			Dispose();
			this.size = size;
			Terrain = new Terrain[size.X,size.Y];
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
