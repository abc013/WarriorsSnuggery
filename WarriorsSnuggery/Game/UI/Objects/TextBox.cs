/*
 * User: Andreas
 * Date: 17.09.2018
 * Time: 15:57
 */
using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class TextBox : Panel
	{
		public bool Selected;
		public string Text
		{
			get
			{
				return realText;
			}
			set
			{
				realText = value;
				text.SetText(realText);
			}
		}
		string realText;
		public readonly int MaximumLength;
		public readonly bool OnlyNumbers;

		bool mouseOnBox;
		readonly TextLine text;
		readonly PanelType type;
		readonly Action onEnter;

		public TextBox(CPos pos, string text, int maximumLength, bool onlyNumbers, PanelType type, Action onEnter) : base(pos, new MPos(8 * maximumLength + 2, 12), type)
		{
			realText = text;
			MaximumLength = maximumLength;
			OnlyNumbers = onlyNumbers;
			this.text = new TextLine(pos + new CPos(128, 0, 0), IFont.Pixel16, Objects.TextLine.OffsetType.MIDDLE);
			this.text.SetText(text);
			this.type = type;
			this.onEnter = onEnter;
		}

		public override void Render()
		{
			HighlightVisible = Selected;
			base.Render();
			text.Render();
		}

		public override void Tick()
		{
			checkMouse();
			if (MouseInput.isLeftClicked)
			{
				Selected = mouseOnBox;
			}

			if (Selected)
			{
				if (KeyInput.IsKeyDown("Enter", 7))
				{
					Selected = false;
					onEnter?.Invoke();
					return;
				}
				if (!OnlyNumbers)
				{
					foreach (var key in KeyInput.AlphabetKeys)
					{
						if (realText.Length <= MaximumLength)
						{
							if (KeyInput.IsKeyDown(key + "", 0))
							{
								var @case = key;
								if (KeyInput.IsKeyDown("ShiftLeft", 0) || KeyInput.IsKeyDown("ShiftRight", 0))
									@case = @case.ToUpper();
								text.AddText(@case);
								realText += @case;
								KeyInput.IsKeyDown(key + "", 7); // To get the delay
							}
						}
					}
				}
				if (KeyInput.IsKeyDown("Back", 7) || KeyInput.IsKeyDown("Delete", 7))
				{
					if (Text.Length != 0)
					{
						realText = Text.Substring(0, Text.Length - 1);
						text.SetText(Text);
					}
				}
				for (int i = 0; i < 10; i++)
				{
					if (KeyInput.IsKeyDown("number" + i, 7))
					{
						if (Text.Length <= MaximumLength)
						{
							text.AddText(i + "");
							realText += i + "";
						}
					}
				}
			}
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;

			var width = (int)((8 * MaximumLength + 2) / 24f * 512);
			var height = 512 / 2;
			mouseOnBox = mousePosition.X > Position.X - width && mousePosition.X < Position.X + width && mousePosition.Y > Position.Y - height && mousePosition.Y < Position.Y + height;
		}
	}
}
