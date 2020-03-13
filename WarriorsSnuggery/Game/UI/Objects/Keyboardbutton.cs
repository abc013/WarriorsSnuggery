using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class KeyboardButton : Panel
	{
		readonly TextLine keyDisplay;

		int blinkTick;

		bool mouseOnBox;
		public bool Selected;

		public string Key;

		public KeyboardButton(CPos position, string key, Color color, PanelType type) : base(position, new Vector(1.5f, 0.25f, 0), type)
		{
			Key = key;

			keyDisplay = new TextLine(position, Font.Pixel16, TextLine.OffsetType.MIDDLE);
			keyDisplay.SetColor(color);
			keyDisplay.SetText(key);
		}

		public override void Render()
		{
			base.Render();
			if (blinkTick < 10)
				keyDisplay.Render();
		}

		public override void Tick()
		{
			base.Tick();

			checkMouse();
			if (mouseOnBox && MouseInput.IsLeftClicked)
				Selected = true;

			else if (MouseInput.IsLeftClicked)
			{
				Selected = false;
				blinkTick = 0;
			}

			if (Selected)
			{
				if (blinkTick-- < 0)
					blinkTick = 20;

				if (Window.KeyInput != OpenTK.Input.Key.End)
				{
					Key = Window.KeyInput.ToString();
					keyDisplay.SetText(Key);
					Selected = false;
					blinkTick = 0;
				}
			}
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;

			var width = 1024;
			var height = 256;
			mouseOnBox = mousePosition.X > Position.X - width && mousePosition.X < Position.X + width && mousePosition.Y > Position.Y - height && mousePosition.Y < Position.Y + height;
		}
	}
}
