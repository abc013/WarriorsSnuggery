using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Objects
{
	public class Button : Panel
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				text.Position = value + new CPos(margin, 0, 0);
			}
		}

		const int margin = UIUtils.TextMargin;

		readonly UITextLine text;
		readonly Action action;

		public Button(string text, string typeName, Action action = null) : this(text, PanelManager.Get(typeName), action) { }

		public Button(string text, PanelType type, Action action = null) : base(new MPos(margin + FontManager.Pixel16.GetWidth(text), margin + FontManager.Pixel16.Height / 2), type)
		{
			this.text = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(margin, 0, 0) };
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
