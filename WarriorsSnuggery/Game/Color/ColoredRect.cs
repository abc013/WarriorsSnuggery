/*
 * User: Andreas
 * Date: 08.10.2017
 * 
 */
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class ColoredRect : PhysicsObject
	{
		public ColoredRect(CPos pos, Color color, float size = 1f, bool isFilled = true) :
			base(pos, new ColoredRectRenderable(color, size, isFilled ? DrawMethod.TRIANGLE : DrawMethod.LINELOOP))
		{ }

		public ColoredRect(CPos pos, Color color, float x, float y, bool isFilled = true) :
			base(pos, new ColoredRectRenderable(color, x, y, isFilled ? DrawMethod.TRIANGLE : DrawMethod.LINELOOP))
		{ }

		public void SetColor(Color color)
		{
			Renderable.SetColor(color);
		}
	}
}
