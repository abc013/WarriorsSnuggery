using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class KeyboardButton : Panel
	{
		const int width = 1024;
		const int height = 256;

		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				keyDisplay.Position = value;
			}
		}

		readonly UITextLine keyDisplay;

		int blinkTick;

		public bool Selected;

		public Keys Key;

		public KeyboardButton(Keys key, Color color, PanelType type) : base(new MPos(width + 512, height), type)
		{
			Key = key;

			keyDisplay = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE)
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

				if (Window.KeyInput != Keys.End)
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
