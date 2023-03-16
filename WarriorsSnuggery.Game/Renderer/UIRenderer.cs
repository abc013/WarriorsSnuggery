using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors.Parts;
using WarriorsSnuggery.UI;

namespace WarriorsSnuggery
{
	public static class UIRenderer
	{
		static Game game;

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

			UICamera.Update();

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

		public static void Render()
		{
			Shader.TextureShader.Uniform(ref UICamera.Matrix, Color.White, CPos.Zero);

			game.ScreenControl.Render();

			var possibleTarget = game.MapType.AllowWeapons && game.World.LocalPlayer != null && game.World.LocalPlayer.GetPart<PlayerPart>().FindValidTarget(MouseInput.GamePosition) != null;
			Cursor.Current = possibleTarget ? CursorType.ATTACK : CursorType.DEFAULT;
			Cursor.Render();

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
