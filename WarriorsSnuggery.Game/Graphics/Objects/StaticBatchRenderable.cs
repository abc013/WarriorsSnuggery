using OpenTK.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public class StaticBatchRenderable
	{
		readonly Vertex[] vertices;
		public bool Visible = true;

		public StaticBatchRenderable(CPos position, VAngle rotation, Texture texture) : this(position, rotation, Mesh.Image(texture)) { }

		public StaticBatchRenderable(CPos position, VAngle rotation, Vertex[] vertices)
		{
			var color = Color.White;
			var vec = position.ToVector();

			var matrix = Matrix4.CreateTranslation(vec.X, vec.Y, vec.Z);
			if (rotation != Vector3.Zero)
			{
				var r1 = Matrix4.CreateRotationX(rotation.X);
				var r2 = Matrix4.CreateRotationY(rotation.Y);
				var r3 = Matrix4.CreateRotationZ(rotation.Z);
				matrix = r1 * r2 * r3 * matrix;
			}

			this.vertices = new Vertex[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
				this.vertices[i] = vertices[i].Apply(matrix, color);
		}

		public virtual void PushToBatchRenderer()
		{
			if (!Visible)
				return;

			MasterRenderer.AddToBatch(vertices);
		}
	}
}
