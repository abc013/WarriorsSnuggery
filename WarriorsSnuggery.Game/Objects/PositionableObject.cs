﻿using WarriorsSnuggery.Graphics;

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

		[Save]
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
			Renderable?.Render();
		}

		public virtual void SetColor(Color color)
		{
			Renderable?.SetColor(color);
		}

		public virtual void SetTextureFlags(TextureFlags flags)
		{
			Renderable?.SetTextureFlags(flags);
		}

		public void RenderShadow()
		{
			if (OnGround || Renderable == null)
				return;

			Renderable.SetPosition(GraphicPositionWithoutHeight);
			Renderable.SetColor(Color.Shadow);
			Renderable.Render();

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

			Visible = CameraVisibility.IsVisible(Position);
			return Visible;
		}

		public virtual void Dispose()
		{
			Disposed = true;
			Visible = false;
		}
	}
}
