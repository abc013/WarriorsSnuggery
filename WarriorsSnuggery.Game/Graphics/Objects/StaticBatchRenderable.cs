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

			this.vertices = new Vertex[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
				this.vertices[i] = vertices[i].Modify(vec, Vector3.One, rotation, color);
		}

		public virtual void PushToBatchRenderer()
		{
			if (!Visible)
				return;

			MasterRenderer.AddToBatch(vertices);
		}
	}
}
