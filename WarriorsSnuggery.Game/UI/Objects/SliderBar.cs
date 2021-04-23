using System;

namespace WarriorsSnuggery.UI.Objects
{
	public class SliderBar : Panel
	{
		readonly Slider slider;

		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				slider.CenterPosition = value;
				slider.Value = slider.Value;
			}
		}

		public float Value
		{
			get => slider.Value;
			set => slider.Value = value;
		}

		public SliderBar(int length, string typeName, Action onChanged = null) : this(length, PanelManager.Get(typeName), onChanged) { }

		public SliderBar(int length, PanelType type, Action onChanged = null) : base(new MPos(length / 2, (int)(1024 * MasterRenderer.PixelMultiplier)), type)
		{
			slider = new Slider(length, type, onChanged);
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

	class Slider : Panel
	{
		public CPos CenterPosition;
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
				Position = new CPos(CenterPosition.X + currentPosition, CenterPosition.Y, 0);
				tooltip = new Tooltip(Math.Round(Value, 1).ToString());
			}
		}

		public Slider(int length, PanelType type, Action onChanged) : base(new MPos((int)(1024 * MasterRenderer.PixelMultiplier), (int)(1024 * 4 * MasterRenderer.PixelMultiplier)), type)
		{
			SelectableBounds = Bounds;
			this.onChanged = onChanged;
			this.length = length / 2;
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
				var xPos = Math.Clamp(MouseInput.WindowPosition.X, CenterPosition.X - length, CenterPosition.X + length);

				currentPosition = xPos - CenterPosition.X;
				Position = new CPos(xPos, Position.Y, Position.Z);

				onChanged?.Invoke();
				UIRenderer.DisableTooltip(tooltip);
				tooltip = new Tooltip(Math.Round(Value, 1).ToString());
				UIRenderer.SetTooltip(tooltip);
			}
		}
	}
}
