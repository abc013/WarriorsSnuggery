using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public class PhysicsObject : ITickRenderDisposable, ICheckVisible, IPositionable
	{
		protected readonly BatchRenderable Renderable;
		public PhysicsSector[] PhysicsSectors = new PhysicsSector[0];
		public readonly SimplePhysics Physics;
		public bool Disposed;

		public int Height
		{
			get { return height; }
			set
			{
				height = value;

				if (Renderable != null)
					Renderable.SetPosition(GraphicPosition);

				if (Physics != null)
					Physics.Height = height;
			}
		}
		int height;

		public virtual CPos Position
		{
			get { return position; }
			set
			{
				position = value;

				if (Renderable != null)
					Renderable.SetPosition(GraphicPosition);

				if (Physics != null)
					Physics.Position = position;
			}
		}
		CPos position;

		public virtual CPos Offset
		{
			get { return offset; }
			set
			{
				offset = value;

				if (Renderable != null)
					Renderable.SetPosition(GraphicPosition);
			}
		}
		CPos offset;

		public virtual CPos GraphicPosition
		{
			get { return new CPos(position.X + offset.X, position.Y + offset.Y - height, position.Z + offset.Z + height); }
			private set { }
		}

		public virtual CPos GraphicPositionWithoutHeight
		{
			get { return new CPos(position.X + offset.X, position.Y + offset.Y, position.Z + offset.Z); }
			private set { }
		}

		public virtual VAngle Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;

				if (Renderable != null)
					Renderable.SetRotation(rotation);
			}
		}
		VAngle rotation;

		public virtual float Scale
		{
			get { return scale; }
			set
			{
				scale = value;

				if (Renderable != null)
					Renderable.SetScale(new Vector(scale, scale, scale));
			}
		}
		float scale = 1f;

		public PhysicsObject(CPos pos)
		{
			Position = pos;
			Physics = SimplePhysics.Empty;
		}

		/// <summary>
		/// Used to create renderables with physical shape.
		/// Used by: Actors, Weapons, Walls, Particles
		/// </summary>
		public PhysicsObject(CPos pos, BatchRenderable renderable, SimplePhysics physics = null)
		{
			Physics = physics ?? SimplePhysics.Empty;
			Renderable = renderable;
			Position = pos;
		}

		public virtual void Tick()
		{

		}

		public virtual void Render()
		{
			if (Renderable != null)
				Renderable.PushToBatchRenderer();
		}

		public virtual void SetColor(Color color)
		{
			if (Renderable != null)
				Renderable.SetColor(color);
		}

		public void RenderShadow()
		{
			if (Height != 0 && Renderable != null)
			{
				MasterRenderer.RenderShadow = true;
				MasterRenderer.UniformHeight(Height);

				Renderable.SetPosition(GraphicPositionWithoutHeight);
				Renderable.SetColor(new Color(0, 0, 0, 64));
				Renderable.PushToBatchRenderer();
				Renderable.SetColor(Color.White);
				Renderable.SetPosition(GraphicPosition);

				MasterRenderer.RenderShadow = false;
				Program.CheckGraphicsError("RenderShadow");
			}
		}

		public virtual void CheckVisibility()
		{
			if (Renderable != null)
				Renderable.Visible = VisibilitySolver.IsVisible(Position);
		}

		public override string ToString()
		{
			return string.Format("GameObject(Disposed={0} | Position={1} | Rotation={2} | Scale={3})", Disposed, position, rotation, scale);
		}

		public virtual void Dispose()
		{
			if (Physics != null)
				Physics.Dispose();
			foreach (var sector in PhysicsSectors)
				sector.Leave(this);

			Disposed = true;
		}
	}
}
