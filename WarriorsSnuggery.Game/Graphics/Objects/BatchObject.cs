namespace WarriorsSnuggery.Graphics
{
	public class BatchObject : BatchRenderable
	{
		public BatchObject(Texture texture) : base(Mesh.Image(texture)) { }

		public BatchObject(Vertex[] vertices) : base(vertices) { }

		public BatchObject(float scale) : base(Mesh.Plane(scale)) { }
	}
}
