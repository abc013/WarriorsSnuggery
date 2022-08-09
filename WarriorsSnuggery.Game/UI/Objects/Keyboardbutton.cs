using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class KeyboardButton : Panel
	{
		const int width = 1024;
		const int height = 256;

		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				keyDisplay.Position = value;
			}
		}

		public override Color Color
		{
			get => base.Color;
			set
			{
				base.Color = value;
				keyDisplay.Color = value;
			}
		}

		readonly UIText keyDisplay;

		int blinkTick;
		bool selected;

		public Keys Key { get; private set; }

		public KeyboardButton(Keys key, PanelType type) : base(new UIPos(width + 512, height), type)
		{
			Key = key;

			keyDisplay = new UIText(FontManager.Default, TextOffset.MIDDLE);
			keyDisplay.SetText(key);

			Bounds = new UIPos(width + 512, height);
			SelectableBounds = new UIPos(width, height);
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

			if (MouseInput.IsLeftClicked)
			{
				selected = ContainsMouse;
				if (selected)
					UIUtils.PlayClickSound();
				else
					blinkTick = 0;
			}

			if (selected && blinkTick-- < 0)
				blinkTick = 20;
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (!selected)
				return;

			UIUtils.PlayClickSound();

			Key = key;
			keyDisplay.SetText(Key);
			selected = false;
			blinkTick = 0;
		}
	}
}
