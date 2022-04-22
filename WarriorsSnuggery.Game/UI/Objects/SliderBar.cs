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

		public SliderBar(int length, string typeName, Action onChanged = null, int tooltipDigits = 1, float valueMultiplier = 1f, bool displayAsPercent = false) : this(length, PanelCache.Types[typeName], onChanged, tooltipDigits, valueMultiplier, displayAsPercent) { }

		public SliderBar(int length, PanelType type, Action onChanged = null, int tooltipDigits = 1, float valueMultiplier = 1f, bool displayAsPercent = false) : base(new MPos(length / 2, (int)(1024 * Constants.PixelMultiplier)), type)
		{
			slider = new Slider(length, type, onChanged, tooltipDigits, valueMultiplier, displayAsPercent);
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

			CheckMouse();
			if (ContainsMouse && MouseInput.IsLeftClicked)
				slider.CalculatePosition();
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
			get => ((currentPosition / (float)length + 1f) / 2) * valueMultiplier;
			set
			{
				value /= valueMultiplier;
				currentPosition = (int)((value - 0.5f) * length) * 2;
				Position = new CPos(CenterPosition.X + currentPosition, CenterPosition.Y, 0);
				tooltip = new Tooltip(Math.Round(displayAsPercent ? Value * 100 : Value, digits).ToString() + (displayAsPercent ? "%" : ""));
			}
		}
		readonly int digits;
		readonly float valueMultiplier;
		readonly bool displayAsPercent;

		public Slider(int length, PanelType type, Action onChanged, int digits = 1, float valueMultiplier = 1f, bool displayAsPercent = false) : base(new MPos((int)(1024 * Constants.PixelMultiplier), (int)(1024 * 4 * Constants.PixelMultiplier)), type)
		{
			SelectableBounds = Bounds;
			this.onChanged = onChanged;
			this.length = length / 2;
			this.digits = digits;
			this.valueMultiplier = valueMultiplier;
			this.displayAsPercent = displayAsPercent;
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
			{
				if (!drag)
					UIUtils.PlayClickSound();
				drag = true;
			}
			else if (!MouseInput.IsLeftDown)
			{
				if (drag)
					UIUtils.PlayClickSound();
				drag = false;
			}

			if (drag)
				CalculatePosition();
		}

		public void CalculatePosition()
		{
			var xPos = Math.Clamp(MouseInput.WindowPosition.X, CenterPosition.X - length, CenterPosition.X + length);

			currentPosition = xPos - CenterPosition.X;
			Position = new CPos(xPos, Position.Y, Position.Z);

			onChanged?.Invoke();
			UIRenderer.DisableTooltip(tooltip);
			tooltip = new Tooltip(Math.Round(displayAsPercent ? Value * 100 : Value, digits).ToString() + (displayAsPercent ? "%" : ""));
			UIRenderer.SetTooltip(tooltip);
		}
	}
}
