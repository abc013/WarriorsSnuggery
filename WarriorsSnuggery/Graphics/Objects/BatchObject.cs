namespace WarriorsSnuggery.Graphics
{
	public class BatchObject : BatchRenderable
	{
		public BatchObject(Texture texture, Color color) : base(Mesh.Image(texture, color), color) { }

		public BatchObject(Vertex[] vertices, Color color) : base(vertices, color) { }

		public BatchObject(float scale, Color color) : base(Mesh.Plane(scale, color), color) { }

		public override void PushToBatchRenderer()
		{
			base.PushToBatchRenderer();
		}
	}
}
