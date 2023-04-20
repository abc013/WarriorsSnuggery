using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class DisplayBar : Panel, ITick
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				text.Position = value;
			}
		}

		readonly Color fillColor;
		readonly UIText text;
		readonly UIParticleManager manager = new UIParticleManager();

		public float DisplayPercentage
		{
			get => displayPercentage;
			set
			{
				if (value < displayPercentage)
					DoDecreaseAnimation(displayPercentage, value);
				else if (value > displayPercentage)
					DoIncreaseAnimation(displayPercentage, value);

				displayPercentage = value;
			}
		}
		float displayPercentage;

		public DisplayBar(UIPos bounds, string typeName, Color fillColor) : this(bounds, PanelCache.Types[typeName], fillColor) { }

		public DisplayBar(UIPos bounds, PanelType type, Color fillColor) : base(bounds, type)
		{
			this.fillColor = fillColor;

			text = new UIText(FontManager.Default, TextOffset.MIDDLE) { Scale = bounds.X / (FontManager.Default.MaxHeight * Constants.PixelMultiplier * Constants.TileSize) };
		}

		public void SetText(string text)
		{
			this.text.SetText(text);
		}

		public override void Render()
		{
			base.Render();

			var offset = Position - SelectableBounds;
			ColorManager.DrawRect(offset, offset + new UIPos((int)(2 * SelectableBounds.X * DisplayPercentage), 2 * SelectableBounds.Y), fillColor);

			manager.Render();
			text.Render();
		}

		public override void DebugRender()
		{
			base.Render();
			manager.DebugRender();
			text.Render();
		}

		public virtual void Tick()
		{
			manager.Tick();
			if (Program.SharedRandom.Next(100) < 3)
			{
				const int radius = 32;

				var xOffset = (int)(2 * SelectableBounds.X * DisplayPercentage);
				var yOffset = Program.SharedRandom.Next(radius * 2, 2 * SelectableBounds.Y - radius * 2);
				var pos = Position - SelectableBounds + new UIPos(xOffset, yOffset);
				var time = (int)MathF.Sqrt(xOffset) * 2;
				var color = (Color color) => {
					var max = MathF.Max(color.R, MathF.Max(color.G, color.B)) / 2;
					return new Color(MathF.Max(color.R, max), MathF.Max(color.G, max), MathF.Max(color.B, max));
				};
				manager.Add(new UIParticle(time) { Color = color(fillColor), Radius = radius * 2, Position = pos, Force = new UIPos(-1, 0) });
			}

			text.Tick();
		}

		void DoIncreaseAnimation(float from, float to)
		{
		}

		void DoDecreaseAnimation(float from, float to)
		{
			const int yCount = 10;
			var radius = SelectableBounds.Y / yCount;
			var xLoss = (int)(2 * SelectableBounds.X * (from - to));
			var xOffset = (int)(2 * SelectableBounds.X * to);
			var xCount = xLoss / (radius * 2) + 1;

			for (int y = 0; y < yCount; y++)
			{
				for (int x = 0; x < xCount; x++)
				{
					var vel = new UIPos(Program.SharedRandom.Next(5, 10), Program.SharedRandom.Next(-10, 10));

					var pos = Position - SelectableBounds + new UIPos(xOffset + (x * 2 + 1) * radius, (y * 2 + 1) * radius);
					var particle = new UIParticle(Settings.UpdatesPerSecond * 3 / 2) { Color = fillColor, Radius = radius * 2, Position = pos, Velocity = new UIPos(vel.X, vel.Y), Force = new UIPos(0, -1) };
					manager.Add(particle);
				}
			}
		}
	}
}
