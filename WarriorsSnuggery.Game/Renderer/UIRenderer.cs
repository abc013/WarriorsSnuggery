using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors.Parts;
using WarriorsSnuggery.UI;

namespace WarriorsSnuggery
{
	public static class UIRenderer
	{
		static Game game;

		static Matrix4 matrix;

		static readonly List<IRenderable> beforeRender = new List<IRenderable>();
		static readonly List<IRenderable> afterRender = new List<IRenderable>();

		static Tooltip tooltip;

		public static Cursor Cursor;

		public static void Initialize()
		{
			Cursor = new Cursor();
		}

		public static void Reset(Game game)
		{
			Cursor.Current = CursorType.DEFAULT;
			UIRenderer.game = game;

			Update();

			beforeRender.Clear();
			afterRender.Clear();
			tooltip = null;
		}

		public static void Update()
		{
			matrix = Matrix4.CreateScale(1 / Camera.UIZoom * 2 / WindowInfo.Ratio, 1 / Camera.UIZoom * 2, 1f);
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
			Shaders.Uniform(Shaders.TextureShader, ref matrix, Color.White);

			foreach (var r in beforeRender)
				r.Render();

			game.ScreenControl.Render();

			foreach (var r in afterRender)
				r.Render();

			var possibleTarget = game.MapType.AllowWeapons && game.World.LocalPlayer != null && game.World.LocalPlayer.GetPart<PlayerPart>().FindValidTarget(MouseInput.GamePosition) != null;

			if (Settings.DeveloperMode)
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

			MasterRenderer.RenderBatch();

			if (Settings.DeveloperMode)
			{
				MasterRenderer.UseDebugRenderer = true;
				MasterRenderer.PrimitiveType = PrimitiveType.Lines;

				game.ScreenControl.DebugRender();

				MasterRenderer.RenderBatch();
				MasterRenderer.PrimitiveType = PrimitiveType.Triangles;
				MasterRenderer.UseDebugRenderer = false;
			}
		}
	}
}
