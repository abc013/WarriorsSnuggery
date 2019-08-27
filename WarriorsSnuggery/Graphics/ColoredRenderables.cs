namespace WarriorsSnuggery.Graphics
{
	public class ColoredCircleRenderable : GraphicsObject
	{
		public ColoredCircleRenderable(Color color, float size, int resolution, DrawMethod method) : base(new IColor(ColoredMesh.Circle(size, color, resolution), method)) { }
	}

	public class ColoredLineRenderable : GraphicsObject
	{
		public ColoredLineRenderable(Color color, float size) : base(new IColor(ColoredMesh.Line(size, color), DrawMethod.LINES)) { }
	}

	class ColoredRectRenderable : GraphicsObject
	{
		public ColoredRectRenderable(Color color, float size, DrawMethod method) : base(new IColor(method == DrawMethod.TRIANGLE ? ColoredMesh.Plane(size, color) : ColoredMesh.PlaneEdges(size, color), method)) { }

		public ColoredRectRenderable(Color color, float x, float y, DrawMethod method) : base(new IColor(method == DrawMethod.TRIANGLE ? ColoredMesh.Plane(x, y, color) : ColoredMesh.PlaneEdges(x, y, color), method)) { }
	}
}
