using OpenTK;
using OpenTK.Graphics.ES30;
using System;

namespace WarriorsSnuggery.Graphics
{
	public abstract class GraphicsObject : IDisposable, IRenderable
	{
		public bool Visible = true;

		protected Vector3 scale = Vector3.One;
		protected Color color = Color.White;
		protected Vector4 position = Vector4.Zero;
		protected Vector4 rotation = Vector4.Zero;
		Matrix4 matrix;

		protected Renderable renderable;

		bool matrixChanged;

		protected GraphicsObject(Renderable renderable)
		{
			this.renderable = renderable;
			matrixChanged = true;
		}

		public void SetPosition(CPos position)
		{
			SetPosition(position.ToVector());
		}

		public void SetPosition(Vector4 position)
		{
			if (this.position == position)
				return;

			this.position = position;
			matrixChanged = true;
		}

		public void SetScale(float scale)
		{
			if (this.scale.Equals(new Vector3(scale)))
				return;

			this.scale = new Vector3(scale);
			matrixChanged = true;
		}

		public void SetScale(CPos scale)
		{
			SetScale(scale.ToVector());
		}

		public void SetScale(Vector scale)
		{
			SetScale(new Vector3(scale.X, scale.Y, scale.Z));
		}

		public void SetScale(Vector3 scale)
		{
			if (this.scale.Equals(scale))
				return;

			this.scale = scale;
			matrixChanged = true;
		}

		public void SetRotation(Vector4 rotation)
		{
			if (this.rotation == rotation)
				return;

			this.rotation = rotation;
			matrixChanged = true;
		}

		public virtual void SetColor(Color color)
		{
			//if (this.color == color)
			//	return; //TODO: does not work red <-> white

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
				GL.UniformMatrix4(MasterRenderer.GetLocation(MasterRenderer.RenderShadow ? MasterRenderer.ShadowShader : renderable.ProgramID, "modelView"), false, ref matrix);
				GL.Uniform4(MasterRenderer.GetLocation(MasterRenderer.RenderShadow ? MasterRenderer.ShadowShader : renderable.ProgramID, "objectColor"), color.toVector4());
				Program.CheckGraphicsError("GraphicsObject_Uniform");
			}
			renderable.Render();
		}

		public virtual void Dispose() //TODO remove
		{
			Visible = false;
			renderable.Dispose();
		}
	}
}
