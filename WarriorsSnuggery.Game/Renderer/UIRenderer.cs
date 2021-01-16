using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public static class UIRenderer
	{
		static Game game;

		static Matrix4 matrix;

		public static readonly BatchRenderer BatchRenderer = new BatchRenderer();
		public static readonly BatchRenderer DebugRenderer = new BatchRenderer();

		static readonly List<IRenderable> beforeRender = new List<IRenderable>();
		static readonly List<IRenderable> afterRender = new List<IRenderable>();

		static Tooltip tooltip;

		public static Cursor Cursor;
		public static void Reset(Game game)
		{
			// This means first reset
			if (Cursor == null)
			{
				Cursor = new Cursor();

				BatchRenderer.SetTextures(SpriteManager.Sheets, SpriteManager.CurrentSheet);
				DebugRenderer.SetTextures(new[] { 0 });
			}

			Cursor.Current = CursorType.DEFAULT;
			UIRenderer.game = game;

			Update();

			ClearRenderLists();
		}

		public static void Update()
		{
			matrix = Matrix4.CreateScale(1 / Camera.UIZoom * 2 / WindowInfo.Ratio, 1 / Camera.UIZoom * 2, 1f);
		}

		public static void ClearRenderLists()
		{
			beforeRender.Clear();
			afterRender.Clear();
			tooltip = null;
		}

		public static void SetTooltip(Tooltip tooltip)
		{
			UIRenderer.tooltip = tooltip;
		}

		public static void DisableTooltip(Tooltip tooltip)
		{
			if (UIRenderer.tooltip == tooltip)
				UIRenderer.tooltip = null;
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

		public static void Render()
		{
			MasterRenderer.Uniform(MasterRenderer.TextureShader, ref matrix, Color.White);

			BatchRenderer.SetCurrent();
			foreach (var r in beforeRender)
				r.Render();

			game.ScreenControl.Render();

			game.RenderDebug();

			foreach (var r in afterRender)
				r.Render();
			var possibleTarget = game.World.LocalPlayer != null && game.World.FindValidTarget(MouseInput.GamePosition, game.World.LocalPlayer.Team) != null;

			if (Settings.EnableDebug)
			{
				ColorManager.DrawRect(new CPos(-64, -64, 0), new CPos(64, 64, 0), Color.Cyan);
				ColorManager.DrawRect(MouseInput.WindowPosition + new CPos(-64, -64, 0), MouseInput.WindowPosition + new CPos(64, 64, 0), possibleTarget ? Color.Red : Color.Blue);
			}
			else
			{
				Cursor.Current = possibleTarget ? CursorType.ATTACK : CursorType.DEFAULT;
				Cursor.Render();
			}

			tooltip?.Render();

			BatchRenderer.Render();
			MasterRenderer.BatchRenderer = null;

			if (Settings.EnableDebug)
			{
				DebugRenderer.SetCurrent();
				MasterRenderer.PrimitiveType = PrimitiveType.Lines;

				game.ScreenControl.DebugRender();

				DebugRenderer.Render();
				MasterRenderer.PrimitiveType = PrimitiveType.Triangles;
				MasterRenderer.BatchRenderer = null;
			}
		}
	}
}
