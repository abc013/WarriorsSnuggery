using System;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class Button : Panel
	{
		readonly TextLine text;
		readonly PanelType type;
		readonly Action action;

		bool mouseOnButton;

		public Button(CPos pos, string text, PanelType type, Action action) : base(pos, new MPos(8 * text.Length + 2, 12), type)
		{
			this.text = new TextLine(pos + new CPos(256,0,0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
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

			mouseOnButton = mousePosition.X > Position.X - type.Width && mousePosition.X < Position.X + type.Width && mousePosition.Y > Position.Y - type.Height && mousePosition.Y < Position.Y + type.Height; // TODO: remove, replace by Physicsaction whatever

			if (MouseInput.isLeftClicked && mouseOnButton && action != null)
				action();
		}
	}
}
