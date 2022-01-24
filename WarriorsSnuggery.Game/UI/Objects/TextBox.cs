﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Linq;
using WarriorsSnuggery.Graphics;

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
            get => textline.Text;
            set => textline.SetText(value);
        }

        public readonly int MaximumLength;
		public readonly InputType Type;

		readonly UIText textline;

		public Action OnEnter;
		public Action OnType;

		public TextBox(string typeName, int maximumLength = 10, InputType type = InputType.NORMAL) : this(PanelCache.Types[typeName], maximumLength, type) { }

		public TextBox(PanelType panelType, int maximumLength = 10, InputType type = InputType.NORMAL) : base(calculateBounds(maximumLength), panelType, true)
		{
			MaximumLength = maximumLength;
			Type = type;

			textline = new UIText(FontManager.Default, TextOffset.LEFT) { Position = new CPos(margin - SelectableBounds.X, 0, 0) };
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
			{
				UIUtils.PlayClickSound();
				Selected = true;
			}
			else if (MouseInput.IsLeftClicked)
				Selected = false;

			HighlightVisible = Selected;

			if (Selected)
			{
				if (Text.Length >= MaximumLength)
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

					UIUtils.PlayClickSound();
					textline.AddText(toAdd);
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
				UIUtils.PlayClickSound();
				Selected = false;
				OnEnter?.Invoke();
				return;
			}

			if (Text.Length > 0 && (key == Keys.Backspace || key == Keys.Delete))
			{
				UIUtils.PlayClickSound();
				textline.SetText(Text[0..^1]);
				OnType?.Invoke();
			}
		}
	}
}
