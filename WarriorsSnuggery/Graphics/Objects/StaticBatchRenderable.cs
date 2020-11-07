using OpenTK.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public class StaticBatchRenderable
	{
		readonly Vertex[] vertices;
		public bool Visible = true;

		public StaticBatchRenderable(CPos position, VAngle rotation, Texture texture, Color color) : this(position, rotation, Mesh.Image(texture), color) { }

		public StaticBatchRenderable(CPos position, VAngle rotation, Vertex[] vertices, Color color)
		{
			var vec = position.ToVector();
			var t2 = Matrix4.CreateTranslation(vec.X, vec.Y, vec.Z);
			var r1 = Matrix4.CreateRotationX(rotation.X);
			var r2 = Matrix4.CreateRotationY(rotation.Y);
			var r3 = Matrix4.CreateRotationZ(rotation.Z);
			var matrix = r1 * r2 * r3 * t2;

			this.vertices = new Vertex[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
				this.vertices[i] = vertices[i].Apply(matrix, color);
		}

		public virtual void PushToBatchRenderer()
		{
			if (!Visible)
				return;

			MasterRenderer.BatchRenderer.Add(vertices);
		}
	}
}
