using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class Button : Panel
	{
		const int margin = UIUtils.TextMargin;

		readonly UITextLine text;
		readonly Action action;

		public Button(CPos pos, string text, string typeName, Action action = null) : this(pos, text, PanelManager.Get(typeName), action) { }

		public Button(CPos pos, string text, PanelType type, Action action = null) : base(pos, new MPos(margin + FontManager.Pixel16.GetWidth(text), margin + FontManager.Pixel16.Height / 2), type)
		{
			this.text = new UITextLine(pos + new CPos(margin, 0, 0), FontManager.Pixel16, TextOffset.MIDDLE);
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

			CheckMouse();

			if (MouseInput.IsLeftClicked && ContainsMouse && action != null)
				action?.Invoke();
		}
	}
}
