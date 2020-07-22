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

		public Button(CPos pos, string text, string type, Action action) : base(pos, new Vector((2 * margin + FontManager.Pixel16.Width * text.Length) / 2048f, (2 * margin + FontManager.Pixel16.Height) / 2048f, 0), PanelManager.Get(type))
		{
			gameBounds = new MPos(FontManager.Pixel16.Width * text.Length / 2 + margin, FontManager.Pixel16.Height / 2 + margin);
			this.text = new TextLine(pos + new CPos(margin, 0, 0), FontManager.Pixel16, TextLine.OffsetType.MIDDLE);
			this.text.WriteText(text);
			this.action = action;
		}

		public override void Render()
		{
			if (ContainsMouse)
			{
				if (MouseInput.IsLeftDown)
					Color = new Color(0.5f, 0.5f, 0.5f);
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

			CheckMouse(gameBounds.X, gameBounds.Y);

			if (MouseInput.IsLeftClicked && ContainsMouse && action != null)
				action();
		}
	}
}
