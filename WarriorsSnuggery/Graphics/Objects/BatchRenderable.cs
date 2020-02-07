using OpenTK;

namespace WarriorsSnuggery.Graphics
{
	public abstract class BatchRenderable
	{
		protected readonly Vertex[] Vertices;
		protected Vertex[] Calculated;

		public bool Visible = true;

		protected Vector4 Position = Vector4.Zero;
		protected Vector4 Rotation = Vector4.Zero;
		protected Vector3 Scale = Vector3.One;
		protected Color Color = Color.White;
		protected bool MatrixChanged;

		public BatchRenderable(Vertex[] vertices, Color color)
		{
			Vertices = vertices;
			Color = color;
			MatrixChanged = true;
		}

		public virtual void SetPosition(CPos position)
		{
			SetPosition(position.ToVector());
		}

		public virtual void SetPosition(Vector position)
		{
			Vector4 vec4 = position;
			if (Position == vec4)
				return;

			Position = vec4;
			MatrixChanged = true;
		}

		public virtual void SetRotation(VAngle rotation)
		{
			SetRotation((Vector4)rotation);
		}

		public virtual void SetRotation(Vector4 rotation)
		{
			Vector4 rot4 = rotation;
			if (Rotation == rot4)
				return;

			Rotation = rot4;
			MatrixChanged = true;
		}

		public virtual void SetScale(float scale)
		{
			SetScale(new Vector3(scale));
		}

		public virtual void SetScale(Vector scale)
		{
			SetScale(((Vector4)scale).Xyz);
		}

		public virtual void SetScale(Vector3 scale)
		{
			if (Scale == scale)
				return;

			Scale = scale;
			MatrixChanged = true;
		}

		public virtual void SetColor(Color color)
		{
			if (Color == color)
				return;

			Color = color;
			MatrixChanged = true;
		}

		public virtual void PushToBatchRenderer()
		{
			if (!Visible)
				return;

			if (MatrixChanged)
			{
				var t2 = Matrix4.CreateTranslation(Position.X, Position.Y, Position.Z);
				var r1 = Matrix4.CreateRotationX(Rotation.X);
				var r2 = Matrix4.CreateRotationY(Rotation.Y);
				var r3 = Matrix4.CreateRotationZ(Rotation.Z);
				var s1 = Matrix4.CreateScale(Scale);
				var matrix = r1 * r2 * r3 * s1 * t2;

				Calculated = new Vertex[Vertices.Length];
				for (int i = 0; i < Vertices.Length; i++)
					Calculated[i] = Vertices[i].Apply(matrix, Color);

				MatrixChanged = false;
			}

			MasterRenderer.BatchRenderer.Add(Calculated);
		}
	}
}
