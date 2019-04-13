using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class ColoredLine : PhysicsObject
	{
		public ColoredLine(CPos pos, Color color, float size = 1f) : 
			base(pos, new ColoredLineRenderable(color, size))
		{ }

		public void SetColor(Color color)
		{
			Renderable.setColor(color);
		}
	}

	class ColoredLineRenderable : GraphicsObject
	{
		public ColoredLineRenderable(Color color, float size) : 
			base(new IColor(ColoredMesh.Line(size, color), DrawMethod.LINES))
		{ }
	}
}
