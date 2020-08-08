using System;

namespace WarriorsSnuggery.UI
{
	public class SliderBar : Panel
	{
		readonly Slider slider;

		public float Value
		{
			get => slider.Value;
			set => slider.Value = value;
		}

		public SliderBar(CPos position, int length, PanelType type, Action onChanged) : base(position, new MPos(length / 2, (int)(1024 * MasterRenderer.PixelMultiplier)), type)
		{
			slider = new Slider(position, length, type, onChanged);
		}

		public override void Render()
		{
			base.Render();

			slider.Render();
		}

		public override void DebugRender()
		{
			base.DebugRender();

			slider.DebugRender();
		}

		public override void Tick()
		{
			base.Tick();

			slider.Tick();
		}
	}

	public class Slider : Panel
	{
		readonly CPos centerPosition;
		readonly int length;

		readonly Action onChanged;

		bool drag;
		int currentPosition;

		Tooltip tooltip;

		public float Value
		{
			get => (currentPosition / (float)length + 1f) / 2;
			set
			{
				currentPosition = (int)((value - 0.5f) * length) * 2;
				Position = new CPos(centerPosition.X + currentPosition, centerPosition.Y, 0);
				tooltip = new Tooltip(Position, Math.Round(Value, 1).ToString());
			}
		}

		public Slider(CPos position, int length, PanelType type, Action onChanged) : base(position, new MPos((int)(1024 * MasterRenderer.PixelMultiplier), (int)(1024 * 4 * MasterRenderer.PixelMultiplier)), type)
		{
			SelectableBounds = Bounds;
			centerPosition = position;
			this.onChanged = onChanged;
			this.length = length / 2;
			tooltip = new Tooltip(position, Math.Round(Value, 1).ToString());
		}

		public override void Tick()
		{
			base.Tick();

			CheckMouse();

			if (ContainsMouse)
				UIRenderer.SetTooltip(tooltip);
			else
				UIRenderer.DisableTooltip(tooltip);

			if (MouseInput.IsLeftDown && ContainsMouse)
				drag = true;
			else if (!MouseInput.IsLeftDown)
				drag = false;

			if (drag)
			{
				var xPos = MouseInput.WindowPosition.X;
				if (xPos < centerPosition.X - length)
					xPos = centerPosition.X - length;

				if (xPos > centerPosition.X + length)
					xPos = centerPosition.X + length;

				currentPosition = xPos - centerPosition.X;
				Position = new CPos(xPos, Position.Y, Position.Z);

				onChanged?.Invoke();
				UIRenderer.DisableTooltip(tooltip);
				tooltip = new Tooltip(Position, Math.Round(Value, 1).ToString());
				UIRenderer.SetTooltip(tooltip);
			}
		}
	}
}
