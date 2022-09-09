using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps.Layers;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery
{
	public static class WorldRenderer
	{
		static Game game;
		static World world;

		public static Color Ambient = Color.White;

		static readonly List<IRenderable> beforeRender = new List<IRenderable>();
		static readonly List<IRenderable> afterRender = new List<IRenderable>();
		readonly static List<PositionableObject> renderables = new List<PositionableObject>();

		public static void Initialize()
		{
			Shroud.Load();
		}

		public static void Reset(Game @new)
		{
			game = @new;
			world = game.World;
			Ambient = world.Map.Type.Ambient;

			CameraVisibility.Reset();
			Camera.Reset();

			ClearRenderLists();
		}

		public static void Render()
		{
			game.LocalRender++;

			if (beforeRender.Count != 0)
			{
				foreach (var o in beforeRender)
					o.Render();

				MasterRenderer.RenderBatch();
			}

			var useAlpha = world.Game.Editor || world.PlayerAlive;

			var pos = CPos.Zero;
			if (useAlpha)
			{
				if (world.Game.Editor)
					pos = MouseInput.GamePosition;
				else
				{
					var localPlayer = world.LocalPlayer;
					pos = localPlayer.GraphicPosition;
					if (localPlayer.Physics != null && !localPlayer.Physics.IsEmpty)
						pos += new CPos(0, world.LocalPlayer.Physics.Boundaries.Y, 0);
				}
			}

			var ambient = Ambient;
			if (Settings.PartyMode)
			{
				var tick = world.Game.LocalTick / 8f;
				var sin1 = MathF.Sin(tick) / 2 + 0.8f;
				var sin2 = MathF.Sin(tick + Angle.MaxRange / 3) / 2 + 0.8f;
				var sin3 = MathF.Sin(tick + 2 * Angle.MaxRange / 3) / 2 + 0.8f;

				ambient *= new Color(sin1, sin2, sin3);
			}

			Shaders.Uniform(Shaders.TextureShader, ref Camera.Matrix, ambient, pos);

			world.TerrainLayer.Render();
			world.SmudgeLayer.Render();

			foreach (var o in prepareRenderList())
			{
				var hiding = false;
				if (useAlpha && o.Position.Y > pos.Y && Math.Abs(o.Position.X - pos.X) < 8144)
				{
					if (o is Actor actor)
						hiding = actor.WorldPart != null && actor.WorldPart.Hideable;
					else if (o is Wall wall)
						hiding = wall.IsHorizontal && wall.Type.Height >= 512;
				}

				if (hiding)
					o.SetTextureFlags(TextureFlags.Hideable);
				o.Render();
				if (hiding)
					o.SetTextureFlags(TextureFlags.None);
			}

			MasterRenderer.RenderBatch();

			Shaders.Uniform(Shaders.TextureShader, ref Camera.Matrix, Color.White, CPos.Zero);
			MasterRenderer.SetRenderer(Renderer.LIGHTS);
			MasterRenderer.RenderBatch();
			MasterRenderer.SetRenderer(Renderer.DEFAULT);

			if (Settings.EnableWeatherEffects)
			{
				world.WeatherManager.Render();
				MasterRenderer.RenderBatch();
			}

			if (afterRender.Count != 0)
			{
				foreach (var o in afterRender)
					o.Render();

				MasterRenderer.RenderBatch();
			}

			if (!world.ShroudLayer.RevealAll)
			{
				CameraVisibility.GetClampedBounds(out var position, out var bounds);

				for (int x = position.X * 2; x < (position.X + bounds.X) * 2; x++)
				{
					for (int y = position.Y * 2; y < (position.Y + bounds.Y) * 2; y++)
						world.ShroudLayer.Shroud[x, y].Render();
				}

				MasterRenderer.RenderBatch();
			}

			if (Settings.DeveloperMode)
			{
				MasterRenderer.SetRenderer(Renderer.DEBUG);

				if (Settings.CurrentMap >= 0 && world.Map.NoiseMaps.ContainsKey(Settings.CurrentMap))
					world.Map.NoiseMaps[Settings.CurrentMap].Render();

				foreach (var point in world.Map.Waypoints)
					ColorManager.DrawDot(point.Position.ToCPos(), Color.Red);

				MasterRenderer.RenderBatch();
				CameraVisibility.GetClampedBounds(out var position, out var bounds);
				var sectorPos = new MPos(position.X / PhysicsLayer.SectorSize, position.Y / PhysicsLayer.SectorSize);
				var sectorBounds = new MPos((int)Math.Ceiling(bounds.X / (float)PhysicsLayer.SectorSize), (int)Math.Ceiling(bounds.Y / (float)PhysicsLayer.SectorSize));

				for (int x = sectorPos.X; x < sectorPos.X + sectorBounds.X; x++)
				{
					for (int y = sectorPos.Y; y < sectorPos.Y + sectorBounds.Y; y++)
						world.PhysicsLayer.Sectors[x, y].RenderDebug();
				}

				world.PathfinderLayer.RenderDebug();

				foreach (var actor in world.ActorLayer.VisibleActors)
					actor.Physics.RenderDebug();

				foreach (var wall in world.WallLayer.VisibleWalls)
					wall.Physics.RenderDebug();

				MasterRenderer.RenderBatch(asLines: true);
				MasterRenderer.SetRenderer(Renderer.DEFAULT);
			}
		}

		static IOrderedEnumerable<PositionableObject> prepareRenderList()
		{
			renderables.Clear();
			renderables.AddRange(world.Objects);
			renderables.AddRange(world.ActorLayer.VisibleActors);
			renderables.AddRange(world.WeaponLayer.VisibleWeapons);
			renderables.AddRange(world.ParticleLayer.VisibleParticles);
			renderables.AddRange(world.WallLayer.VisibleWalls);

			return renderables.OrderBy(e => e.GraphicPosition.Z + (e.Position.Y - 512) * 2);
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
			foreach (Terrain t in world.TerrainLayer.Terrain)
				t.CheckVisibility(true);

			world.WallLayer.CheckVisibility();
			world.ActorLayer.CheckVisibility();
			world.ParticleLayer.CheckVisibility();
			world.WeaponLayer.CheckVisibility();

			world.SmudgeLayer.CheckVisibility();

			foreach (var o in world.Objects)
				o.CheckVisibility();
		}

		public static void CheckVisibility(CPos oldPos, CPos newPos, bool tinyMove = true)
		{
			var zoom = Camera.CurrentZoom;

			if (tinyMove)
			{
				var diff = oldPos - newPos;
				var greaterDiff = Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));
				var checkZoom = zoom + greaterDiff / 2048f;
				CheckVisibility(oldPos + diff / 2, checkZoom);
			}
			else
			{
				CheckVisibility(oldPos, zoom);
				CheckVisibility(newPos, zoom);
			}
		}

		public static void CheckVisibility(float oldZoom, float newZoom)
		{
			var zoom = Math.Max(oldZoom, newZoom);
			CheckVisibility(Camera.LookAt, zoom);
		}

		public static void CheckVisibility(CPos pos, float zoom)
		{
			var zoomPos = new CPos((int)(zoom * WindowInfo.Ratio * 512), (int)(zoom * 512), 0);

			var margin = new CPos(Settings.VisibilityMargin, Settings.VisibilityMargin, 0);
			var topLeft = pos - zoomPos - margin;
			var bottomRight = pos + zoomPos + margin;
			check(topLeft, bottomRight);

			CameraVisibility.GetClampedBounds(out var position, out var bounds);

			for (int x = position.X; x < position.X + bounds.X; x++)
			{
				for (int y = position.Y; y < position.Y + bounds.Y; y++)
					world.TerrainLayer.Terrain[x, y].CheckVisibility();
			}

			world.WallLayer.CheckVisibility(position, position + bounds);
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
