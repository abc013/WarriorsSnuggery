using OpenTK;

namespace WarriorsSnuggery.Graphics
{
	public class BatchObject
	{
		readonly Vertex[] vertices;
		Vertex[] calculated;

		public bool Visible = true;

		Vector4 position = Vector4.Zero;
		Vector4 rotation = Vector4.Zero;
		Vector3 scale = Vector3.One;
		Color color = Color.White;
		bool matrixChanged;

		public BatchObject(ITexture texture, Color color)
		{
			this.color = color;
			vertices = Mesh.Image(texture, color);
		}

		public BatchObject(Color color)
		{
			this.color = color;
			vertices = Mesh.Plane(1f, color);
		}

		public void SetPosition(CPos position)
		{
			SetPosition(position.ToVector());
		}

		public void SetPosition(Vector position)
		{
			Vector4 vec4 = position;
			if (this.position == vec4)
				return;

			this.position = vec4;
			matrixChanged = true;
		}

		public void SetRotation(VAngle rotation)
		{
			Vector4 rot4 = rotation;
			if (this.rotation == rot4)
				return;

			this.rotation = rot4;
			matrixChanged = true;
		}

		public void SetScale(Vector scale)
		{
			Vector3 sca3 = ((Vector4)scale).Xyz;
			if (this.scale == sca3)
				return;

			this.scale = sca3;
			matrixChanged = true;
		}

		public void SetColor(Color color)
		{
			if (this.color == color)
				return;

			this.color = color;
			matrixChanged = true;
		}

		public void PushToBatchRenderer()
		{
			if (!Visible)
				return;

			if (matrixChanged)
			{
				var t2 = Matrix4.CreateTranslation(position.X, position.Y, position.Z);
				var r1 = Matrix4.CreateRotationX(rotation.X);
				var r2 = Matrix4.CreateRotationY(rotation.Y);
				var r3 = Matrix4.CreateRotationZ(rotation.Z);
				var s1 = Matrix4.CreateScale(scale);
				var matrix = r1 * r2 * r3 * s1 * t2;

				calculated = new Vertex[vertices.Length];
				for (int i = 0; i < vertices.Length; i++)
					calculated[i] = vertices[i].Apply(matrix, color);

				matrixChanged = false;
			}

			MasterRenderer.BatchRenderer.Add(calculated);
		}
	}
}
