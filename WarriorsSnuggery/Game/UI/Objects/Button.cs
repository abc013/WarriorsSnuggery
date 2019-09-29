/*
 * User: Andreas
 * Date: 18.04.2018
 */

using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class Button : Panel
	{
		readonly TextLine text;
		readonly Action action;

		bool mouseOnButton;

		public Button(CPos pos, string text, PanelType type, Action action) : base(pos, new MPos(8 * text.Length + 2, 12), type)
		{
			this.text = new TextLine(pos + new CPos(256, 0, 0), IFont.Pixel16, TextLine.OffsetType.MIDDLE);
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

			mouseOnButton = mousePosition.X > Position.X - Bounds.X && mousePosition.X < Position.X + Bounds.X && mousePosition.Y > Position.Y - Bounds.Y && mousePosition.Y < Position.Y + Bounds.Y;

			if (MouseInput.IsLeftClicked && mouseOnButton && action != null)
				action();
		}
	}
}
