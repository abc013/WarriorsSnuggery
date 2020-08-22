using OpenToolkit.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public static class WorldRenderer
	{
		static Game game;
		static World world;

		static BatchObject shroud;

		public static readonly BatchRenderer BatchRenderer = new BatchRenderer();
		public static readonly BatchRenderer DebugRenderer = new BatchRenderer();

		public static Color Ambient = Color.White;

		static readonly List<IRenderable> beforeRender = new List<IRenderable>();
		static readonly List<IRenderable> afterRender = new List<IRenderable>();

		public static void Reset(Game @new)
		{
			// This means first reset
			if (shroud == null)
			{
				shroud = new BatchObject(RuleLoader.ShroudTexture[0], Color.White);

				BatchRenderer.SetTextures(SpriteManager.Sheets, SpriteManager.CurrentSheet);
				DebugRenderer.SetTextures(new[] { 0 });
			}
			game = @new;
			world = game.World;
			BatchRenderer.Clear();

			VisibilitySolver.Reset();
			Camera.Reset();

			ClearRenderLists();
		}

		public static void Render()
		{
			game.LocalRender++;

			BatchRenderer.SetCurrent();
			foreach (var o in beforeRender)
				o.Render();
			BatchRenderer.Render();

			MasterRenderer.Uniform(MasterRenderer.TextureShader, ref Camera.Matrix, Ambient);

			if (world.ToRender == null)
				return;

			world.TerrainLayer.Render();
			BatchRenderer.Render();

			world.SmudgeLayer.Render();
			BatchRenderer.Render();

			foreach (var o in world.ToRender)
			{
				CPos pos = world.Game.Editor ? MouseInput.GamePosition : world.LocalPlayer == null ? CPos.Zero : world.LocalPlayer.Position;
				if (((o is Actor actor && actor.WorldPart != null && actor.WorldPart.Hideable) || (o is Wall wall && wall.LayerPosition.X % 2 != 0 && wall.Type.Height >= 512)) && o.Position.Y > pos.Y && Math.Abs(o.Position.X - pos.X) < 4096)
				{
					var alpha = o.Position.Y - pos.Y < 1024 ? 1 - (o.Position.Y - pos.Y) / 1024f : (o.Position.Y - pos.Y - 1024) / 1024f;
					var sidealpha = Math.Abs(o.Position.X - pos.X) / 4096f;
					if (sidealpha > alpha)
						alpha = sidealpha;
					alpha = Math.Clamp(alpha, 0.5f, 1f);

					o.SetColor(new Color(1f, 1f, 1f, alpha));
					o.Render();
					o.SetColor(Color.White);
				}
				else
					o.Render();
			}
			BatchRenderer.Render();

			foreach (var o in afterRender)
				o.Render();
			BatchRenderer.Render();

			if (!world.ShroudLayer.RevealAll)
			{
				var bounds = VisibilitySolver.GetBounds(out var position);

				for (int x = (position.X) * 2; x < (position.X + bounds.X) * 2; x++)
				{
					if (x >= 0 && x < world.ShroudLayer.Bounds.X)
					{
						for (int y = (position.Y) * 2; y < (position.Y + bounds.Y) * 2; y++)
						{
							if (y >= 0 && y < world.ShroudLayer.Bounds.Y)
							{
								var alpha = world.ShroudLayer.ShroudAlpha(new MPos(x, y), Actor.PlayerTeam);
								if (alpha > 0f)
								{
									shroud.SetColor(new Color(1f, 1f, 1f, alpha));
									shroud.SetPosition(new CPos(x * 512 - 256, y * 512 - 256, 0));
									shroud.PushToBatchRenderer();
								}
							}
						}
					}
				}
			}
			BatchRenderer.Render();
			MasterRenderer.BatchRenderer = null;

			if (Settings.DeveloperMode)
			{
				DebugRenderer.SetCurrent();
				if (world.Map.Noises.Any())
				{
					world.Map.Noises.First().Value.Render();
					DebugRenderer.Render();
				}
				MasterRenderer.PrimitiveType = PrimitiveType.Lines;
				foreach (var sector in world.PhysicsLayer.Sectors)
					sector.RenderDebug();

				foreach (var wall in world.WallLayer.Walls)
				{
					if (wall != null)
						wall.Physics.RenderDebug();
				}

				DebugRenderer.Render();
				MasterRenderer.BatchRenderer = null;
				MasterRenderer.PrimitiveType = PrimitiveType.Triangles;
			}

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

		public static void CheckVisibilityAll()
		{
			checkAllTerrain(true);
			checkAll();
			checkAllWalls();
		}

		public static void CheckVisibility(CPos oldPos, CPos newPos)
		{
			var zoom = Camera.CurrentZoom;

			CheckVisibility(oldPos, zoom);
			CheckVisibility(newPos, zoom);
		}

		public static void CheckVisibility(float oldZoom, float newZoom)
		{
			var zoom = Math.Max(oldZoom, newZoom);
			CheckVisibility(Camera.LookAt, zoom);
		}

		public static void CheckVisibility(CPos pos, float zoom)
		{
			var zoomPos = new CPos((int)(zoom * WindowInfo.Ratio * 512), (int)(zoom * 512), 0);
			// TODO determine margins in some way
			var topLeft = pos - zoomPos - new CPos(3072, 3072, 0);
			var bottomRight = pos + zoomPos + new CPos(3072, 3072, 0);
			check(topLeft, bottomRight);

			var botLeft = VisibilitySolver.LookAt(pos, zoom);
			var topRight = botLeft + VisibilitySolver.Zoom(zoom);

			if (botLeft.X < 0) botLeft = new MPos(0, botLeft.Y);
			if (botLeft.X > world.Map.Bounds.X) botLeft = new MPos(world.Map.Bounds.X, botLeft.Y);
			if (botLeft.Y < 0) botLeft = new MPos(botLeft.X, 0);
			if (botLeft.Y > world.Map.Bounds.Y) botLeft = new MPos(botLeft.X, world.Map.Bounds.Y);

			if (topRight.X < 0) topRight = new MPos(0, topRight.Y);
			if (topRight.X > world.Map.Bounds.X) topRight = new MPos(world.Map.Bounds.X, topRight.Y);
			if (topRight.Y < 0) topRight = new MPos(topRight.X, 0);
			if (topRight.Y > world.Map.Bounds.Y) topRight = new MPos(topRight.X, world.Map.Bounds.Y);

			checkTerrain(botLeft, topRight);
			checkWalls(botLeft, topRight);
		}

		static void checkAllWalls()
		{
			world.WallLayer.CheckVisibility();
		}

		static void checkWalls(MPos bottomleft, MPos topright)
		{
			world.WallLayer.CheckVisibility(bottomleft, topright);
		}

		static void checkAll()
		{
			world.ActorLayer.CheckVisibility();
			world.ParticleLayer.CheckVisibility();
			world.WeaponLayer.CheckVisibility();

			world.SmudgeLayer.CheckVisibility();

			foreach (var o in world.Objects)
				o.CheckVisibility();
		}

		static void check(CPos topLeft, CPos bottomRight)
		{
			world.ActorLayer.CheckVisibility(topLeft, bottomRight);
			world.ParticleLayer.CheckVisibility(topLeft, bottomRight);
			world.WeaponLayer.CheckVisibility(topLeft, bottomRight);

			world.SmudgeLayer.CheckVisibility(topLeft, bottomRight);

			var objects = world.Objects.Where(a => a.GraphicPosition.X > topLeft.X && a.GraphicPosition.X < bottomRight.X && a.GraphicPosition.Y > topLeft.Y && a.GraphicPosition.Y < bottomRight.Y);
			foreach (var o in objects)
				o.CheckVisibility();

			// TODO: check smudge as well
		}

		static void checkAllTerrain(bool checkEdges = false)
		{
			if (world.TerrainLayer == null)
				return;

			foreach (Terrain t in world.TerrainLayer.Terrain)
				t.CheckVisibility(checkEdges);
		}

		static void checkTerrain(MPos bottomleft, MPos topright, bool checkEdges = false)
		{
			if (world.TerrainLayer == null)
				return;

			for (int x = bottomleft.X; x < topright.X; x++)
			{
				for (int y = bottomleft.Y; y < topright.Y; y++)
					world.TerrainLayer.Terrain[x, y].CheckVisibility(checkEdges);
			}
		}

		public static void CheckTerrainAround(MPos pos, bool checkEdges = false)
		{
			int calls = 0;
			for (int x = pos.X - 1; x < pos.X + 2; x++)
			{
				if (x >= 0 && x < world.Map.Bounds.X)
				{
					for (int y = pos.Y - 1; y < pos.Y + 2; y++)
					{
						if (y >= 0 && y < world.Map.Bounds.Y)
						{
							calls++;
							world.TerrainLayer.Terrain[x, y].CheckVisibility(checkEdges);
						}
					}
				}
			}
		}
	}
}
