using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class Panel : IRenderable, ITick, IDisposable
	{
		public virtual CPos Position
		{
			get { return position; }
			set
			{
				inner.setPosition(value);
				outer.setPosition(value);
				if (Highlight != null)
					Highlight.setPosition(value);
				position = value;
			}
		}
		CPos position;

		public virtual CPos Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;
				inner.setRotation(rotation.ToAngle());
				outer.setRotation(rotation.ToAngle());
				if (Highlight != null) // int is degree
					Highlight.setRotation(rotation.ToAngle());
			}
		}
		CPos rotation;

		public virtual float Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				inner.setScale(scale);
				outer.setScale(scale);
				if (Highlight != null)
					Highlight.setScale(scale);
			}
		}
		float scale = 1f;

		readonly GraphicsObject inner;
		readonly GraphicsObject outer;
		public readonly GraphicsObject Highlight;

		public bool HighlightVisible;

		public Panel(CPos position, MPos size, int border, string inner, string outer, string highlight) : this(position, size, border, inner, outer, highlight != "" ? new ImageRenderable(TextureManager.Texture(highlight), size) : null)
		{
		}

		public Panel(CPos position, MPos size, int border, string inner, string outer, ImageRenderable highlight)
		{
			this.inner = new ImageRenderable(TextureManager.Texture(inner), size);
			this.outer = new ImageRenderable(TextureManager.Texture(outer), size + new MPos(border, border));
			if (highlight != null) Highlight = highlight;

			Position = position;
		}

		public virtual void Render()
		{
			outer.Render();
			inner.Render();

			if (HighlightVisible && Highlight != null)
				Highlight.Render();
		}

		public virtual void Tick()
		{

		}

		public virtual void Dispose()
		{
			// TODO: needs dispose?
		}
	}
}