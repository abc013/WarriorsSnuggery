using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public static class WorldRenderer
	{
		static Game game;
		static World world;
		
		public static Color Ambient = Color.White;

		static readonly List<IRenderable> beforeRender = new List<IRenderable>();
		static readonly List<IRenderable> afterRender = new List<IRenderable>();

		public static void Reset(Game @new)
		{
			game = @new;
			world = game.World;
			Camera.Reset();
			ClearRenderLists();
		}

		public static void Render()
		{
			game.LocalRender++;

			foreach (var o in beforeRender)
				o.Render();

			MasterRenderer.Uniform(MasterRenderer.TextureShader, ref Camera.Matrix, Ambient);
			MasterRenderer.Uniform(MasterRenderer.ColorShader, ref Camera.Matrix, Ambient);
			MasterRenderer.Uniform(MasterRenderer.FontShader, ref Camera.Matrix, Ambient);
			MasterRenderer.Uniform(MasterRenderer.ShadowShader, ref Camera.Matrix, Ambient);

			if (world.ToRender == null)
				return;

			world.TerrainLayer.Render();

			foreach (PhysicsObject o in world.ToRender)
			{
				CPos pos = world.Game.Editor ? MouseInput.GamePosition : world.LocalPlayer == null ? CPos.Zero : world.LocalPlayer.Position;
				if (((o is Actor && ((Actor) o).WorldPart != null && ((Actor)o).WorldPart.Hideable) || (o is Wall && ((Wall)o).Type.Height >= 512)) && o.Position.Y > pos.Y && Math.Abs(o.Position.X - pos.X) < 4096) // TODO: not very effective. ADD game.mouseposition so that walls under mouse become alpha too?
				{
					var alpha = o.Position.Y - pos.Y < 1024 ? 1 - (o.Position.Y - pos.Y) / 1024f : (o.Position.Y - pos.Y - 1024) / 1024f;
					var sidealpha = Math.Abs(o.Position.X - pos.X) / 4096f;
					if (sidealpha > alpha)
						alpha = sidealpha;
					if (alpha < 0.5f) alpha = 0.5f;
					if (alpha > 1f) alpha = 1f;

					Ambient = new Color(Ambient.R, Ambient.G, Ambient.B, alpha);
					MasterRenderer.Uniform(MasterRenderer.TextureShader, ref Camera.Matrix, Ambient);
					MasterRenderer.Uniform(MasterRenderer.ColorShader, ref Camera.Matrix, Ambient);
					MasterRenderer.Uniform(MasterRenderer.FontShader, ref Camera.Matrix, Ambient);
					MasterRenderer.Uniform(MasterRenderer.ShadowShader, ref Camera.Matrix, Ambient);
					o.Render();

					Ambient = new Color(Ambient.R, Ambient.G, Ambient.B, 1f);
					MasterRenderer.Uniform(MasterRenderer.TextureShader, ref Camera.Matrix, Ambient);
					MasterRenderer.Uniform(MasterRenderer.ColorShader, ref Camera.Matrix, Ambient);
					MasterRenderer.Uniform(MasterRenderer.FontShader, ref Camera.Matrix, Ambient);
					MasterRenderer.Uniform(MasterRenderer.ShadowShader, ref Camera.Matrix, Ambient);
				}
				else
					o.Render();
			}

			foreach (var o in afterRender)
				o.Render();

			Ambient = world.Map.Type.Ambient;
		}

		public static void ClearRenderLists()
		{
			beforeRender.Clear();
			afterRender.Clear();
		}

		public static void RenderAfter(IRenderable renderable)
		{
			afterRender.Add(renderable);
		}

		public static void RenderBefore(IRenderable renderable)
		{
			beforeRender.Add(renderable);
		}

		public static void RemoveRenderAfter(IRenderable renderable)
		{
			afterRender.Remove(renderable);
		}

		public static void RemoveRenderBefore(IRenderable renderable)
		{
			beforeRender.Remove(renderable);
		}

		public static void CheckObjectVisibility()
		{
			CheckTerrainVisibility(game.Editor);
			CheckActorVisibility();
			CheckWallVisibility();
		}

		public static void CheckWallVisibility()
		{
			if (world.WallLayer == null)
				return;

			foreach (Wall w in world.WallLayer.Walls)
				if (w != null) w.CheckVisibility();
		}

		public static void CheckTerrainVisibility(bool checkEdges = false)
		{
			if (world.TerrainLayer == null)
				return;

			foreach(Terrain t in world.TerrainLayer.Terrain)
				t.CheckVisibility(checkEdges);
		}

		public static void CheckActorVisibility()
		{
			foreach(Actor a in world.Actors)
				a.CheckVisibility();
			foreach(PhysicsObject o in world.Objects)
				o.CheckVisibility();
		}
	}
}
