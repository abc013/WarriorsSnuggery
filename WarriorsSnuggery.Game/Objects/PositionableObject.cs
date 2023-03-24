using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class PositionableObject : ITick, IRenderable
	{
		protected virtual BatchRenderable Renderable
		{
			get => renderable;
			set
			{
				renderable = value;

				renderable?.SetPosition(Position);
				renderable?.SetScale(Scale);
				renderable?.SetRotation(Rotation);
				renderable?.SetColor(Color);
				renderable?.SetTextureFlags(TextureFlags);
			}
		}
		BatchRenderable renderable;
		public bool Disposed;

		public virtual CPos Position
		{
			get => position;
			set
			{
				// make it impossible to be underground.
				if (value.Z < 0)
					value = new CPos(value.X, value.Y, 0);

				position = value;

				Renderable?.SetPosition(GraphicPosition);
			}
		}
		CPos position;

		public bool OnGround => Position.Z == 0;

		public virtual int ZOffset
		{
			get => zOffset;
			set
			{
				zOffset = value;

				Renderable?.SetPosition(GraphicPosition);
			}
		}
		int zOffset;

		public virtual CPos GraphicPositionWithoutHeight => position + new CPos(0, 0, zOffset);
		public virtual CPos GraphicPosition => GraphicPositionWithoutHeight + new CPos(0, -position.Z, 0);

		public virtual VAngle Rotation
		{
			get => rotation;
			set
			{
				rotation = value;

				Renderable?.SetRotation(rotation);
			}
		}
		VAngle rotation;

		public virtual float Scale
		{
			get => scale;
			set
			{
				scale = value;

				Renderable?.SetScale(new Vector(scale, scale, scale));
			}
		}
		float scale = 1f;

		public virtual Color Color
		{
			get => color;
			set
			{
				color = value;

				Renderable?.SetColor(color);
			}
		}
		Color color = Color.White;

		public virtual TextureFlags TextureFlags
		{
			get => textureFlags;
			set
			{
				textureFlags = value;

				Renderable?.SetTextureFlags(textureFlags);
			}
		}
		TextureFlags textureFlags = TextureFlags.None;

		public PositionableObject() { }

		public virtual void Tick()
		{
			Renderable?.Tick();
		}

		public virtual void Render()
		{
			Renderable?.Render();
		}

		public void RenderShadow()
		{
			if (OnGround || Renderable == null)
				return;

			Renderable.SetPosition(GraphicPositionWithoutHeight);
			Renderable.SetColor(Color.Shadow);
			Renderable.SetTextureFlags(TextureFlags.None);
			Renderable.Render();

			Renderable.SetPosition(GraphicPosition);
			Renderable.SetColor(Color);
			Renderable.SetTextureFlags(TextureFlags);
		}

		public virtual void Dispose()
		{
			Disposed = true;
		}
	}
}
