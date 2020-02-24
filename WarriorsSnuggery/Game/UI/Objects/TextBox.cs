using System;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class TextBox : Panel
	{
		const int margin = 64;

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
		public readonly bool IsPath;

		bool mouseOnBox;
		readonly TextLine text;
		readonly MPos gameBounds;

		public Action OnEnter;
		public Action OnType;

		public TextBox(CPos pos, string text, int maximumLength, bool onlyNumbers, bool isPath, PanelType type) : base(pos, new Vector((2 * margin + Font.Pixel16.Width * maximumLength) / 2048f, (2 * margin + Font.Pixel16.Height) / 2048f, 0), type)
		{
			gameBounds = new MPos((Font.Pixel16.Width * maximumLength) / 2 + margin, Font.Pixel16.Height / 2 + margin);
			realText = text;
			MaximumLength = maximumLength;
			OnlyNumbers = onlyNumbers;
			IsPath = isPath;
			this.text = new TextLine(pos + new CPos(128, 0, 0), Font.Pixel16, Objects.TextLine.OffsetType.MIDDLE);
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
			checkMouse();
			if (MouseInput.IsLeftClicked)
				Selected = mouseOnBox;

			if (Selected)
			{
				if (KeyInput.IsKeyDown("Enter", 7))
				{
					Selected = false;
					OnEnter?.Invoke();
					return;
				}
				if (Text.Length > 0 && (KeyInput.IsKeyDown("Back", 5) || KeyInput.IsKeyDown("Delete", 5)))
				{
					realText = Text.Substring(0, Text.Length - 1);
					text.SetText(Text);
					OnType?.Invoke();
					return;
				}
				if (realText.Length <= MaximumLength && Window.CharInput != '')
				{
					if (OnlyNumbers && !int.TryParse(Window.CharInput + "", out _))
						return;

					if (IsPath && KeyInput.InvalidFileNameChars.Contains(Window.CharInput))
						return;

					text.AddText(Window.CharInput);
					realText += Window.CharInput;
					OnType?.Invoke();
				}
			}
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;
			mouseOnBox = mousePosition.X > Position.X - gameBounds.X && mousePosition.X < Position.X + gameBounds.X && mousePosition.Y > Position.Y - gameBounds.Y && mousePosition.Y < Position.Y + gameBounds.Y;
		}
	}
}
