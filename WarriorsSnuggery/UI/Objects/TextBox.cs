using OpenToolkit.Windowing.Common.Input;
using System;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class TextBox : Panel
	{
		const int margin = UIUtils.TextMargin;

		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				if (text != null)
					text.Position = value + new CPos(128, 0, 0);
			}
		}

		public bool Selected;
		public string Text
		{
			get => realText;
			set
			{
				realText = value;
				text.SetText(realText);
			}
		}
		string realText;
		public readonly int MaximumLength;
		public readonly bool OnlyNumbers;
		public readonly bool IsPath;

		readonly UITextLine text;

		public Action OnEnter;
		public Action OnType;

		public TextBox(CPos pos, string text, string type, int maximumLength = 10, bool onlyNumbers = false, bool isPath = false) : base(pos, new MPos(margin + FontManager.Pixel16.Width * maximumLength / 2, margin + FontManager.Pixel16.Height / 2), PanelManager.Get(type))
		{
			realText = text;
			MaximumLength = maximumLength;
			OnlyNumbers = onlyNumbers;
			IsPath = isPath;
			this.text = new UITextLine(pos + new CPos(128, 0, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			this.text.SetText(text);
		}

		public override void Render()
		{
			HighlightVisible = Selected;
			base.Render();
			text.Render();
		}

		public override void Tick()
		{
			CheckMouse();
			if (ContainsMouse && MouseInput.IsLeftClicked)
				Selected = true;
			else if (MouseInput.IsLeftClicked)
				Selected = false;

			if (Selected)
			{
				if (KeyInput.IsKeyDown(Key.Enter, 7))
				{
					Selected = false;
					OnEnter?.Invoke();
					return;
				}
				if (Text.Length > 0 && (KeyInput.IsKeyDown(Key.Back, 5) || KeyInput.IsKeyDown(Key.Delete, 5)))
				{
					realText = Text[0..^1];
					text.SetText(Text);
					OnType?.Invoke();
					return;
				}

				var input = Window.StringInput;
				if (realText.Length < MaximumLength && !string.IsNullOrEmpty(input))
				{
					if (OnlyNumbers && !int.TryParse(input + "", out _))
						return;

					var toAdd = input;
					if (IsPath)
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

					text.AddText(toAdd);
					realText += toAdd;
					OnType?.Invoke();
				}
			}
		}
	}
}
