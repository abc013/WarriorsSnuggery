using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public sealed class Button : Panel, ITick
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				text.Position = value + new UIPos(margin, 0);
			}
		}

		const int margin = UIUtils.TextMargin;

		readonly UIText text;
		readonly Action action;

		bool containsMouse;

		public Button(string text, string typeName, Action action = null) : this(text, PanelCache.Types[typeName], action) { }

		public Button(string text, PanelType type, Action action = null) : base(new UIPos(margin + FontManager.Default.Measure(text).width / 2, margin + FontManager.Default.Measure(text).height / 2), type, true)
		{
			this.text = new UIText(FontManager.Default, TextOffset.MIDDLE) { Position = new UIPos(margin, 0) };
			this.text.SetText(text);
			this.action = action;
		}

		public override void Render()
		{
			if (containsMouse)
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

		public void Tick()
		{
			containsMouse = UIUtils.ContainsMouse(this);

			if (MouseInput.IsLeftClicked && containsMouse && action != null)
			{
				UIUtils.PlayClickSound();
				action?.Invoke();
			}
		}
	}
}
