using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class ColoredCircle : GameObject
	{
		public ColoredCircle(CPos pos, Color color, float size = 1f, int resolution = 45, bool isFilled = true) : 
			base(pos, new ColoredCircleRenderable(color, size, resolution, isFilled ? DrawMethod.TRIANGLEFAN: DrawMethod.LINELOOP))
		{ }

		public void setColor(Color color)
		{
			Renderable.setColor(color);
		}
	}
	
	class ColoredCircleRenderable : GraphicsObject
	{
		public ColoredCircleRenderable(Color color, float size, int resolution, DrawMethod method) : 
			base(new IColor(ColoredMesh.Circle(size, color, resolution), method))
		{ }
	}
}
