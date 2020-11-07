using OpenTK.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public abstract class BatchRenderable
	{
		readonly Vertex[] vertices;
		readonly Vertex[] calculated;

		public bool Visible = true;

		protected Vector3 Position = Vector3.Zero;
		protected Vector3 Rotation = Vector3.Zero;
		protected Vector3 Scale = Vector3.One;
		protected Color Color = Color.White;
		protected bool MatrixChanged;

		public BatchRenderable(Vertex[] vertices, Color color)
		{
			this.vertices = vertices;
			calculated = new Vertex[vertices.Length];
			Color = color;
			MatrixChanged = true;
		}

		public virtual void SetPosition(CPos position)
		{
			SetPosition(position.ToVector());
		}

		public virtual void SetPosition(Vector position)
		{
			Vector3 vec3 = position;
			if (Position == vec3)
				return;

			Position = vec3;
			MatrixChanged = true;
		}

		public virtual void SetRotation(VAngle rotation)
		{
			SetRotation((Vector3)rotation);
		}

		public virtual void SetRotation(Vector3 rotation)
		{
			Vector3 rot3 = rotation;
			if (Rotation == rot3)
				return;

			Rotation = rot3;
			MatrixChanged = true;
		}

		public virtual void SetScale(float scale)
		{
			SetScale(new Vector3(scale));
		}

		public virtual void SetScale(Vector scale)
		{
			SetScale((Vector3)scale);
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

		public virtual void Tick() { }

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

				for (int i = 0; i < vertices.Length; i++)
					calculated[i] = vertices[i].Apply(matrix, Color);

				MatrixChanged = false;
			}

			MasterRenderer.BatchRenderer.Add(calculated);
		}
	}
}
