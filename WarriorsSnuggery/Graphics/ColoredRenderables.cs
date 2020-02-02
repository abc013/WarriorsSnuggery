namespace WarriorsSnuggery.Graphics
{
	public class ColoredCircleRenderable : GraphicsObject
	{
		public ColoredCircleRenderable(Color color, float size, int resolution, DrawMethod method) : base(new IColor(Mesh.Circle(size, color, resolution))) { }
	}

	public class ColoredRectRenderable : GraphicsObject
	{
		public ColoredRectRenderable(IColor color) : base(color) { }

		public ColoredRectRenderable(Color color, float size, DrawMethod method) : base(new IColor(method == DrawMethod.TRIANGLE ? Mesh.Plane(size, color) : Mesh.PlaneEdges(size, color))) { }

		public ColoredRectRenderable(Color color, float x, float y, DrawMethod method) : base(new IColor(method == DrawMethod.TRIANGLE ? Mesh.Plane(x, y, color) : Mesh.PlaneEdges(x, y, color))) { }
	}
}
