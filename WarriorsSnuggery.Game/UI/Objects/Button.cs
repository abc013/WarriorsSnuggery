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
				text.Position = value + new CPos(margin / 2, 0, 0);
			}
		}

		const int margin = UIUtils.TextMargin;

		readonly UITextLine text;
		readonly Action action;

		public Button(string text, string typeName, Action action = null) : this(text, PanelManager.Get(typeName), action) { }

		public Button(string text, PanelType type, Action action = null) : base(new MPos(margin + FontManager.Default.GetWidth(text) / 2, margin / 2 + FontManager.Default.MaxHeight / 2), type, true)
		{
			this.text = new UITextLine(FontManager.Default, TextOffset.MIDDLE) { Position = new CPos(margin / 2, 0, 0) };
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
