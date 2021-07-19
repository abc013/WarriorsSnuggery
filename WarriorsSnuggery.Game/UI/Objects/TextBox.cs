using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Objects
{
	public enum InputType
	{
		NORMAL,
		NUMBERS,
		PATH
	}

	public class TextBox : Panel
	{
		const int margin = UIUtils.TextMargin;

		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				textline.Position = value + new CPos(margin - SelectableBounds.X, 0, 0);
			}
		}

		public bool Selected;
		public string Text
		{
			get => text;
			set
			{
				text = value;
				textline.SetText(text);
			}
		}
		string text = string.Empty;
		public readonly int MaximumLength;
		public readonly InputType Type;

		readonly UITextLine textline;

		public Action OnEnter;
		public Action OnType;

		public TextBox(string typeName, int maximumLength = 10, InputType type = InputType.NORMAL) : this(PanelManager.Types[typeName], maximumLength, type) { }

		public TextBox(PanelType panelType, int maximumLength = 10, InputType type = InputType.NORMAL) : base(calculateBounds(maximumLength), panelType, true)
		{
			MaximumLength = maximumLength;
			Type = type;

			textline = new UITextLine(FontManager.Default, TextOffset.LEFT) { Position = new CPos(margin - SelectableBounds.X, 0, 0) };
		}

		static MPos calculateBounds(int maximumLength)
		{
			// 3 is an approximation that looks pretty good
			return new MPos(margin + FontManager.Default.MaxWidth * maximumLength / 3, margin / 2 + FontManager.Default.MaxHeight / 2);
		}

		public override void Render()
		{
			base.Render();
			textline.Render();
		}

		public override void Tick()
		{
			CheckMouse();
			if (ContainsMouse && MouseInput.IsLeftClicked)
				Selected = true;
			else if (MouseInput.IsLeftClicked)
				Selected = false;

			HighlightVisible = Selected;

			if (Selected)
			{
				if (text.Length >= MaximumLength)
					return;

				var input = KeyInput.Text;
				if (!string.IsNullOrEmpty(input))
				{
					var toAdd = input;
					if (Type == InputType.NUMBERS && !int.TryParse(input, out _))
						return;

					if (Type == InputType.PATH)
					{
						toAdd = string.Empty;

						foreach (var @char in input)
						{
							if (!KeyInput.InvalidFileNameChars.Contains(@char))
								toAdd += @char;
						}

						if (string.IsNullOrEmpty(toAdd))
							return;
					}

					textline.AddText(toAdd);
					text += toAdd;
					OnType?.Invoke();
				}
			}
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (!Selected)
				return;

			if (key == Keys.Enter)
			{
				Selected = false;
				OnEnter?.Invoke();
				return;
			}

			if (Text.Length > 0 && (key == Keys.Backspace || key == Keys.Delete))
			{
				text = Text[0..^1];
				textline.SetText(Text);
				OnType?.Invoke();
			}
		}
	}
}
