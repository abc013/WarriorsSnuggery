using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class Button : Panel
	{
		readonly TextLine text;
		readonly Action action;
		readonly MPos gameBounds;

		bool mouseOnButton;

		public Button(CPos pos, string text, PanelType type, Action action) : base(pos, new Vector(Font.Pixel16.Info.Size * text.Length * 0.5f * MasterRenderer.PixelMultiplier, Font.Pixel16.Info.Size * MasterRenderer.PixelMultiplier * 0.7f, 0), type)
		{
			gameBounds = new MPos((int)(Font.Pixel16.Info.Size * 0.5f * text.Length * MasterRenderer.PixelMultiplier * 1024), (int)(Font.Pixel16.Info.Size * MasterRenderer.PixelMultiplier * 0.7f * 1024));
			this.text = new TextLine(pos + new CPos(256, 0, 0), Font.Pixel16, TextLine.OffsetType.MIDDLE);
			this.text.WriteText(text);
			this.action = action;
		}

		public override void Render()
		{
			if (mouseOnButton)
			{
				if (MouseInput.IsLeftDown)
				{
					Color = new Color(0.5f, 0.5f, 0.5f);
				}
				else
				{
					Color = Color.White;
					HighlightVisible = true;
				}
			}
			else
			{
				Color = Color.White;
				HighlightVisible = false;
			}

			base.Render();
			text.Render();
		}

		public override void Tick()
		{
			base.Tick();

			checkMouse();
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;

			mouseOnButton = mousePosition.X > Position.X - gameBounds.X && mousePosition.X < Position.X + gameBounds.X && mousePosition.Y > Position.Y - gameBounds.Y && mousePosition.Y < Position.Y + gameBounds.Y;

			if (MouseInput.IsLeftClicked && mouseOnButton && action != null)
				action();
		}
	}
}
