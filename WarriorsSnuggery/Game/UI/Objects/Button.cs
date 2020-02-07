using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class Button : Panel
	{
		const int margin = 64;

		readonly TextLine text;
		readonly Action action;
		readonly MPos gameBounds;

		bool mouseOnButton;

		public Button(CPos pos, string text, PanelType type, Action action) : base(pos, new Vector((2 * margin + Font.Pixel16.Width * text.Length) / 2048f, (2 * margin + Font.Pixel16.Height) / 2048f, 0), type)
		{
			gameBounds = new MPos((Font.Pixel16.Width * text.Length) / 2 + margin, Font.Pixel16.Height / 2 + margin);
			this.text = new TextLine(pos + new CPos(margin, 0, 0), Font.Pixel16, TextLine.OffsetType.MIDDLE);
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
