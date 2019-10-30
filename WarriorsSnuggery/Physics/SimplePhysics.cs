/*
 * User: Andreas
 * Date: 17.06.2018
 * Time: 14:10
 */
using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Physics
{
	public sealed class SimplePhysics : IDisposable
	{
		public static readonly SimplePhysics Empty = new SimplePhysics(CPos.Zero, 0, Shape.NONE, 0, 0, 0);

		public readonly Shape Shape;
		public readonly int RadiusX;
		public readonly int RadiusY;// only for box and drawing
		public readonly int HeightRadius;

		readonly GraphicsObject renderable;

		public CPos Position;
		public int Height;

		public SimplePhysics(CPos position, int height, Shape shape, int radiusX, int radiusY, int heightradius)
		{
			Position = position;
			Height = height;
			Shape = shape;
			RadiusX = radiusX;
			RadiusY = radiusY;
			HeightRadius = heightradius;

			if (!Settings.DeveloperMode)
				return;

			switch (Shape)
			{
				case Shape.CIRCLE:
					renderable = new ColoredCircleRenderable(Color.Cyan, RadiusX * 2 / 1024f, 16, DrawMethod.LINELOOP);
					renderable.SetPosition(Position);
					break;
				case Shape.RECTANGLE:
					renderable = new ColoredRectRenderable(Color.Cyan, RadiusX * 2 / 1024f, RadiusY * 2 / 1024f, DrawMethod.LINELOOP);
					renderable.SetPosition(Position);
					break;
				case Shape.LINE_HORIZONTAL:
					renderable = new ColoredLineRenderable(Color.Cyan, RadiusX * 2 / 1024f);
					renderable.SetPosition(Position - new CPos(0, RadiusY, -10240));
					renderable.SetRotation(new VAngle(0, 0, 90));
					break;
				case Shape.LINE_VERTICAL:
					renderable = new ColoredLineRenderable(Color.Cyan, RadiusX * 2 / 1024f);
					renderable.SetPosition(Position - new CPos(RadiusX, 0, 0));
					break;
			}
			if (renderable != null)
			{
				WorldRenderer.RenderAfter(renderable);
			}
		}

		public bool Intersects(SimplePhysics other, bool ignoreHeight)
		{
			if (other == null)
				return false;

			if (Shape == Shape.NONE || other.Shape == Shape.NONE)
				return false;

			if (!ignoreHeight && Math.Abs(other.Height - Height) >= other.HeightRadius + HeightRadius)
				return false;

			if (other.Shape != Shape.LINE_HORIZONTAL && Shape != Shape.LINE_HORIZONTAL && other.Shape != Shape.LINE_VERTICAL && Shape != Shape.LINE_VERTICAL)
			{
				if (Math.Abs(other.Position.X - Position.X) >= other.RadiusX + RadiusX)
					return false;

				if (Math.Abs(other.Position.Y - Position.Y) >= other.RadiusX + RadiusX)
					return false;
			}

			if (Shape == Shape.CIRCLE && other.Shape == Shape.CIRCLE)
			{
				return Position.DistToXY(other.Position) <= RadiusX + other.RadiusX;
			}
			if (Shape == Shape.RECTANGLE && other.Shape == Shape.RECTANGLE)
			{

				var diff = other.Position - Position;
				var scaleX = RadiusX + other.RadiusX;
				var scaleY = RadiusY + other.RadiusY;
				return Math.Abs(diff.X) < scaleX && Math.Abs(diff.Y) < scaleY;
			}

			// CIRCLE <-> BOX
			if (Shape == Shape.CIRCLE && other.Shape == Shape.RECTANGLE || Shape == Shape.RECTANGLE && other.Shape == Shape.CIRCLE)
			{
				var circle = Shape == Shape.CIRCLE ? this : other;
				var box = circle == this ? other : this;
				var pos = circle.Position - box.Position;
				pos = new CPos(Math.Abs(pos.X), Math.Abs(pos.Y), Math.Abs(pos.Z));

				if (pos.X > (box.RadiusX + circle.RadiusX)) return false;
				if (pos.Y > (box.RadiusY + circle.RadiusY)) return false;

				if (pos.X <= box.RadiusX) return true;
				if (pos.Y <= box.RadiusY) return true;

				// Pythagorean theorem: We calculate X and Y in order to get the circle distance; Added error margin "box.Radius/8".
				var corner = (pos.X - box.RadiusX) ^ 2 + (pos.Y - box.RadiusY) ^ 2 - box.RadiusX / 8;

				return corner <= (circle.RadiusX ^ 2);
			}

			// CIRCLE <-> LINE
			if (Shape == Shape.CIRCLE && other.Shape == Shape.LINE_VERTICAL || Shape == Shape.LINE_VERTICAL && other.Shape == Shape.CIRCLE)
			{
				var circle = Shape == Shape.CIRCLE ? this : other;
				var line = circle == this ? other : this;

				if (Math.Abs(circle.Position.Y - line.Position.Y + 512) > circle.RadiusX + line.RadiusX) return false;

				return Math.Abs(circle.Position.X - line.Position.X + 512) <= circle.RadiusX;
			}

			if (Shape == Shape.CIRCLE && other.Shape == Shape.LINE_HORIZONTAL || Shape == Shape.LINE_HORIZONTAL && other.Shape == Shape.CIRCLE)
			{
				var circle = Shape == Shape.CIRCLE ? this : other;
				var line = circle == this ? other : this;

				if (Math.Abs(circle.Position.X - line.Position.X + 512) > circle.RadiusX + line.RadiusX) return false;

				return Math.Abs(circle.Position.Y - line.Position.Y + 512) <= circle.RadiusX;
			}

			// BOX <-> LINE
			if (Shape == Shape.RECTANGLE && other.Shape == Shape.LINE_HORIZONTAL || Shape == Shape.LINE_HORIZONTAL && other.Shape == Shape.RECTANGLE)
			{
				var box = Shape == Shape.RECTANGLE ? this : other;
				var line = box == this ? other : this;

				if (Math.Abs(box.Position.Y - line.Position.Y + 512) > line.RadiusX) return false;
				if (Math.Abs(box.Position.X - line.Position.X + 512) > (box.RadiusY + line.RadiusX)) return false;

				return true;
			}
			if (Shape == Shape.RECTANGLE && other.Shape == Shape.LINE_VERTICAL || Shape == Shape.LINE_VERTICAL && other.Shape == Shape.RECTANGLE)
			{
				var box = Shape == Shape.RECTANGLE ? this : other;
				var line = box == this ? other : this;

				if (Math.Abs(box.Position.X - line.Position.X + 512) > line.RadiusX) return false;
				if (Math.Abs(box.Position.Y - line.Position.Y + 512) > (box.RadiusY + line.RadiusX)) return false;

				return true;
			}

			return false;
		}

		public PhysicsLine[] GetLines()
		{
			switch(Shape)
			{
				default:
					return new PhysicsLine[0];
				case Shape.LINE_VERTICAL:
					return new PhysicsLine[]
					{
						new PhysicsLine(Position - new CPos(RadiusX, 2 * RadiusY, 0), Position + new CPos(-RadiusX, 0, 0))
					};
				case Shape.LINE_HORIZONTAL:
					return new PhysicsLine[]
					{
						new PhysicsLine(Position - new CPos(2 * RadiusX, RadiusY, 0), Position + new CPos(0, -RadiusY, 0))
					};
					// TODO: give Circle an own collider
				case Shape.CIRCLE:
				case Shape.RECTANGLE:
					return new PhysicsLine[]
					{
						new PhysicsLine(Position - new CPos(RadiusX, RadiusY, 0), Position + new CPos(-RadiusX, RadiusY, 0)),
						new PhysicsLine(Position - new CPos(RadiusX, RadiusY, 0), Position + new CPos(RadiusX, -RadiusY, 0)),
						new PhysicsLine(Position + new CPos(-RadiusX, RadiusY, 0), Position + new CPos(RadiusX, RadiusY, 0)),
						new PhysicsLine(Position + new CPos(RadiusX, -RadiusY, 0), Position + new CPos(RadiusX, RadiusY, 0))
					};
			}
		}

		public void RenderShape()
		{
			if (renderable == null)
				return;

			switch (Shape)
			{
				default:
					renderable.SetPosition(Position);
					break;
				case Shape.LINE_HORIZONTAL:
					renderable.SetPosition(Position - new CPos(0, RadiusY, 0));
					break;
				case Shape.LINE_VERTICAL:
					renderable.SetPosition(Position - new CPos(RadiusX, 0, 0));
					break;
			}
		}

		public void Dispose()
		{
			if (renderable != null)
			{
				WorldRenderer.RemoveRenderAfter(renderable);
				renderable.Dispose();
			}
		}
	}
}
