using OpenTK.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public abstract class BatchRenderable
	{
		readonly Vertex[] vertices;
		readonly Vertex[] calculated;

		public bool Visible = true;

		protected Vector3 Position = Vector3.Zero;
		protected Quaternion Rotation = Quaternion.Identity;
		protected Vector3 Scale = Vector3.One;
		protected Color Color = Color.White;
		protected TextureFlags TextureFlags = TextureFlags.None;
		protected bool CacheOutdated;

		public BatchRenderable(Vertex[] vertices)
		{
			this.vertices = vertices;
			calculated = new Vertex[vertices.Length];

			CacheOutdated = true;
		}

		public void SetPosition(CPos position)
		{
			SetPosition(position.ToVector());
		}

		public virtual void SetPosition(Vector position)
		{
			Vector3 vec3 = position;
			if (Position == vec3)
				return;

			Position = vec3;
			CacheOutdated = true;
		}

		public void SetRotation(VAngle rotation)
		{
			SetRotation((Quaternion)rotation);
		}

		public virtual void SetRotation(Quaternion rot3)
		{
			if (Rotation == rot3)
				return;

			Rotation = rot3;
			CacheOutdated = true;
		}

		public void SetScale(float scale)
		{
			SetScale(new Vector3(scale));
		}

		public void SetScale(Vector scale)
		{
			SetScale((Vector3)scale);
		}

		public virtual void SetScale(Vector3 scale)
		{
			if (Scale == scale)
				return;

			Scale = scale;
			CacheOutdated = true;
		}

		public virtual void SetColor(Color color)
		{
			if (Color == color)
				return;

			Color = color;
			CacheOutdated = true;
		}

		public virtual void SetTextureFlags(TextureFlags textureFlags)
		{
			if (TextureFlags == textureFlags)
				return;

			TextureFlags = textureFlags;
			CacheOutdated = true;
		}

		public virtual void Tick() { }

		public virtual void Render()
		{
			if (!Visible)
				return;

			if (CacheOutdated)
			{
				for (int i = 0; i < vertices.Length; i++)
					calculated[i] = vertices[i].Modify(Position, Scale, Rotation, Color, TextureFlags);

				CacheOutdated = false;
			}

			MasterRenderer.AddToBatch(calculated);
		}
	}
}
