using System;
using OpenTK;
using OpenTK.Graphics.ES30;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public abstract class GraphicsObject : IDisposable, IRenderable
	{
		public bool Visible = true;

		protected Vector3 scale = Vector3.One;
		protected Color color = Color.White;
		protected Vector4 position = Vector4.Zero;
		protected Vector4 rotation = Vector4.Zero;
		protected Matrix4 matrix;

		protected readonly Renderable renderable;

		bool matrixChanged;

		protected GraphicsObject(Renderable renderable)
		{
			this.renderable = renderable;
			matrixChanged = true;
		}

		public void setPosition(CPos position)
		{
			setPosition(position.ToVector());
		}

		public void setPosition(Vector4 position)
		{
			if (this.position == position)
				return;

			this.position = position;
			matrixChanged = true;
		}

		public void setScale(float scale)
		{
			if (this.scale.Equals(new Vector3(scale)))
				return;

			this.scale = new Vector3(scale);
			matrixChanged = true;
		}

		public void setRotation(Vector4 rotation)
		{
			if (this.rotation == rotation)
				return;

			this.rotation = rotation;
			matrixChanged = true;
		}

		public virtual void setColor(Color color)
		{
			//if (this.color == color)
			//	return;

			this.color = color;
		}

		void updateMatrix()
		{
			var t2 = Matrix4.CreateTranslation(position.X, position.Y, position.Z);
			var r1 = Matrix4.CreateRotationX(rotation.X);
			var r2 = Matrix4.CreateRotationY(rotation.Y);
			var r3 = Matrix4.CreateRotationZ(rotation.Z);
			var s1 = Matrix4.CreateScale(scale);
			matrix = r1 * r2 * r3 * s1 * t2;
			matrixChanged = false;
		}

		public virtual void Render()
		{
			if (!Visible)
				return;

			if (matrixChanged)
				updateMatrix();

			renderable.Bind();
			lock (MasterRenderer.GLLock)
			{
				GL.UniformMatrix4(MasterRenderer.GetLocation(renderable.ProgramID, "modelView"), false, ref matrix);
				GL.Uniform4(MasterRenderer.GetLocation(renderable.ProgramID, "objectColor"), color.toVector4());
			}
			renderable.Render();
		}

		public virtual void Dispose()
		{
			Visible = false;
			renderable.Dispose();
		}

		public bool CheckVisibility(CPos pos)
		{
			return Visible = Camera.IsVisible(pos, scale.X, scale.Y);
		}

		public bool CheckVisibility()
		{
			return Visible = Camera.IsVisible(position, scale.X, scale.Y);
		}
	}
}
