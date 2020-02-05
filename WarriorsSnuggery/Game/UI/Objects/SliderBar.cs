using System;

namespace WarriorsSnuggery.UI
{
	public class SliderBar : Panel
	{
		readonly Slider slider;

		public float Value
		{
			get { return slider.Value; }
			set { slider.Value = value; }
		}

		public SliderBar(CPos position, int size, PanelType type) : base(position, new Vector(size * MasterRenderer.PixelMultiplier, 2 * MasterRenderer.PixelMultiplier, 0), type)
		{
			slider = new Slider(position, size, type);
		}

		public override void Render()
		{
			base.Render();

			slider.Render();
		}

		public override void Tick()
		{
			base.Tick();

			slider.Tick();
		}

		public override void Dispose()
		{
			base.Dispose();

			slider.Dispose();
		}
	}

	public class Slider : Panel
	{
		readonly CPos centerPosition;
		readonly int limit;
		readonly MPos gameBounds;

		bool mouseOnSlider;
		bool drag;
		int currentPosition;

		Tooltip tooltip;

		public float Value
		{
			get { return (currentPosition / (float)limit + 1f) / 2; }
			set
			{
				currentPosition = (int)((value - 0.5f) * limit) * 2;
				Position = new CPos(centerPosition.X + currentPosition, centerPosition.Y, 0);
				tooltip = new Tooltip(Position, Math.Round(Value, 1).ToString());
			}
		}

		public Slider(CPos position, int limit, PanelType type) : base(position, new Vector(2 * MasterRenderer.PixelMultiplier, 8 * MasterRenderer.PixelMultiplier, 0), type)
		{
			gameBounds = new MPos((int)(2 * MasterRenderer.PixelMultiplier * 1024), (int)(8 * MasterRenderer.PixelMultiplier * 1024));
			centerPosition = position;
			this.limit = (int)(limit * MasterRenderer.PixelMultiplier * 512);
			tooltip = new Tooltip(position, Math.Round(Value, 1).ToString());
		}

		public override void Tick()
		{
			base.Tick();

			checkMouse();

			if (drag)
			{
				var xPos = MouseInput.WindowPosition.X;
				if (xPos < centerPosition.X - limit)
					xPos = centerPosition.X - limit;

				if (xPos > centerPosition.X + limit)
					xPos = centerPosition.X + limit;

				currentPosition = xPos - centerPosition.X;
				Position = new CPos(xPos, Position.Y, Position.Z);

				UIRenderer.DisableTooltip(tooltip);
				tooltip = new Tooltip(Position, Math.Round(Value, 1).ToString());
			}
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;
			mouseOnSlider = mousePosition.X > Position.X - gameBounds.X && mousePosition.X < Position.X + gameBounds.X && mousePosition.Y > Position.Y - gameBounds.Y && mousePosition.Y < Position.Y + gameBounds.Y;

			if (mouseOnSlider)
				UIRenderer.SetTooltip(tooltip);
			else
				UIRenderer.DisableTooltip(tooltip);

			if (MouseInput.IsLeftDown && mouseOnSlider)
				drag = true;
			else if (!MouseInput.IsLeftDown)
				drag = false;
		}

		public override void Dispose()
		{
			base.Dispose();
			tooltip.Dispose();
		}
	}
}
