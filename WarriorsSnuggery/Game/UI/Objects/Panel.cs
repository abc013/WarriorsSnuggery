using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class Panel : IPositionable, ITickRenderDisposable
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

		readonly BatchObject background;
		readonly BatchObject border;
		public readonly BatchObject Highlight;

		public bool HighlightVisible;

		public Panel(CPos position, Vector bounds, PanelType type) : this(position, bounds, type, type.Background2 != null ? new BatchObject(Mesh.UIPlane(type.Background2, Color.White, bounds), Color.White) : null) { }

		public Panel(CPos position, Vector bounds, PanelType type, BatchObject background2)
		{
			background = new BatchObject(Mesh.UIPlane(type.Background, Color.White, bounds), Color.White);
			border = new BatchObject(Mesh.UIPlane(type.Border, Color.White, bounds + new Vector(type.BorderWidth, type.BorderWidth, 0)), Color.White);
			if (background2 != null) Highlight = background2;

			Bounds = new MPos((int)((bounds.X + type.BorderWidth) * MasterRenderer.PixelMultiplier * 512), (int)((bounds.Y + type.BorderWidth) * MasterRenderer.PixelMultiplier * 512));

			Position = position;
		}

		public virtual void Render()
		{
			border.PushToBatchRenderer();
			background.PushToBatchRenderer();

			if (HighlightVisible && Highlight != null)
				Highlight.PushToBatchRenderer();
		}

		public virtual void Tick() { }

		public virtual void Dispose() { }
	}
}