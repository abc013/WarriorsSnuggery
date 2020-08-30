using OpenToolkit.Windowing.Common.Input;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class KeyboardButton : Panel
	{
		const int width = 1024;
		const int height = 256;

		readonly UITextLine keyDisplay;

		int blinkTick;

		public bool Selected;

		public Key Key;

		public KeyboardButton(CPos position, Key key, Color color, PanelType type) : base(position, new MPos(width + 512, height), type)
		{
			Key = key;

			keyDisplay = new UITextLine(position, FontManager.Pixel16, TextOffset.MIDDLE)
			{
				Color = color
			};
			keyDisplay.SetText(key);

			Bounds = new MPos(width + 512, height);
			SelectableBounds = new MPos(width, height);
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

			CheckMouse();

			if (ContainsMouse && MouseInput.IsLeftClicked)
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

				if (Window.KeyInput != Key.End)
				{
					Key = Window.KeyInput;
					keyDisplay.SetText(Key);
					Selected = false;
					blinkTick = 0;
				}
			}
		}
	}
}
