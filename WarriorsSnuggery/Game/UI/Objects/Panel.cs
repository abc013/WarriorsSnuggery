using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class Panel : IPositionable, ITickRenderable, IDisposable
	{
		public virtual CPos Position
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
				background.SetPosition(position);
				border.SetPosition(position);
				if (Highlight != null)
					Highlight.SetPosition(position);
			}
		}
		CPos position;

		public virtual VAngle Rotation
		{
			get
			{
				return rotation;
			}
			set
			{
				rotation = value;
				background.SetRotation(rotation);
				border.SetRotation(rotation);
				if (Highlight != null)
					Highlight.SetRotation(rotation);
			}
		}
		VAngle rotation;

		public virtual float Scale
		{
			get
			{
				return scale;
			}
			set
			{
				scale = value;
				background.SetScale(scale);
				border.SetScale(scale);
				if (Highlight != null)
					Highlight.SetScale(scale);
			}
		}
		float scale = 1f;

		public virtual Color Color
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
				background.SetColor(color);
				border.SetColor(color);
				if (Highlight != null)
					Highlight.SetColor(color);
			}
		}
		Color color = Color.White;

		public virtual MPos Bounds
		{
			get; private set;
		}

		readonly GraphicsObject background;
		readonly GraphicsObject border;
		public readonly GraphicsObject Highlight;

		public bool HighlightVisible;

		public Panel(CPos position, MPos bounds, PanelType type) : this(position, bounds, type.BorderWidth, type.Background, type.Border, type.Background2 != "" ? new ImageRenderable(TextureManager.Texture(type.Background2), bounds) : null) { }

		public Panel(CPos position, MPos bounds, int borderWidth, string background, string border, ImageRenderable background2)
		{
			this.background = new ImageRenderable(TextureManager.Texture(background), bounds);
			this.border = new ImageRenderable(TextureManager.Texture(border), bounds + new MPos(borderWidth, borderWidth));
			if (background2 != null) Highlight = background2;

			Bounds = new MPos((int)((bounds.X + borderWidth) * MasterRenderer.PixelMultiplier * 512), (int)((bounds.Y + borderWidth) * MasterRenderer.PixelMultiplier * 512));

			Position = position;
		}

		public virtual void Render()
		{
			border.Render();
			background.Render();

			if (HighlightVisible && Highlight != null)
				Highlight.Render();
		}

		public virtual void Tick() { }

		public virtual void Dispose() { }
	}
}