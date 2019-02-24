using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class Button : Panel
	{
		readonly Text text;
		readonly ButtonType type;
		readonly Action action;

		bool mouseOnButton;

		public Button(CPos pos, string text, ButtonType type, Action action) : base(pos, new MPos(8 * text.Length + 2, 12), type.Border, type.DefaultString, type.BorderString, type.ActiveString)
		{
			this.text = new Text(pos + new CPos(256,0,0), IFont.Pixel16, Text.OffsetType.MIDDLE);
			this.text.WriteText(text);
			this.type = type;
			this.action = action;
		}

		public override void Render()
		{
			if (mouseOnButton)
			{
				if (MouseInput.isLeftDown)
				{
					// TODO: add click here.
				}
				else
				{
					HighlightVisible = true;
				}
			}
			else
			{
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

			mouseOnButton = mousePosition.X > Position.X - type.Width && mousePosition.X < Position.X + type.Width && mousePosition.Y > Position.Y - type.Height && mousePosition.Y < Position.Y + type.Height;

			if (MouseInput.isLeftClicked && mouseOnButton && action != null)
				action();
		}
	}
}
