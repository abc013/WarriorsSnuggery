using OpenTK.Mathematics;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors.Parts;
using WarriorsSnuggery.UI;

namespace WarriorsSnuggery
{
	public static class UIRenderer
	{
		static Game game;

		static Matrix4 matrix;

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

		public static void Render()
		{
			Shaders.Uniform(Shaders.TextureShader, ref matrix, Color.White, CPos.Zero);

			game.ScreenControl.Render();

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
				MasterRenderer.SetRenderer(Renderer.DEBUG);

				game.ScreenControl.DebugRender();

				MasterRenderer.RenderBatch(asLines: true);
				MasterRenderer.SetRenderer(Renderer.DEFAULT);
			}
		}
	}
}
