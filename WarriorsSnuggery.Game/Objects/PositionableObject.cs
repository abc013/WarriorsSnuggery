using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class PositionableObject : ITickRenderable, ICheckVisible, IPositionable
	{
		protected readonly BatchRenderable Renderable;
		public bool Disposed;

		public virtual bool Visible
		{
			get => visible;
			set
			{
				if (Renderable != null)
					Renderable.Visible = value;

				visible = value;
			}
		}
		bool visible = true;

		public virtual int Height
		{
			get => height;
			set
			{
				height = value;

				Renderable?.SetPosition(GraphicPosition);
			}
		}
		int height;

		public virtual CPos Position
		{
			get => position;
			set
			{
				position = value;

				Renderable?.SetPosition(GraphicPosition);
			}
		}
		CPos position;

		public virtual CPos Offset
		{
			get => offset;
			set
			{
				offset = value;

				Renderable?.SetPosition(GraphicPosition);
			}
		}
		CPos offset;

		public virtual CPos GraphicPosition => new CPos(position.X + offset.X, position.Y + offset.Y - height, position.Z + offset.Z + height);

		public virtual CPos GraphicPositionWithoutHeight => new CPos(position.X + offset.X, position.Y + offset.Y, position.Z + offset.Z);

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

		public PositionableObject(CPos pos, BatchRenderable renderable)
		{
			Renderable = renderable;
			Position = pos;
		}

		public virtual void Tick()
		{
			Renderable?.Tick();
		}

		public virtual void Render()
		{
			Renderable?.PushToBatchRenderer();
		}

		public virtual void SetColor(Color color)
		{
			Renderable?.SetColor(color);
		}

		public void RenderShadow()
		{
			if (Height <= 0 || Renderable == null)
				return;

			Renderable.SetPosition(GraphicPositionWithoutHeight);
			Renderable.SetColor(new Color(0, 0, 0, 64));
			Renderable.PushToBatchRenderer();
			Renderable.SetColor(Color.White);
			Renderable.SetPosition(GraphicPosition);
		}

		public virtual bool CheckVisibility()
		{
			if (Renderable == null || Disposed)
			{
				Visible = false;
				return Visible;
			}

			Visible = VisibilitySolver.IsVisible(Position);
			return Visible;
		}

		public virtual void Dispose()
		{
			Disposed = true;
			Visible = false;
		}
	}
}
