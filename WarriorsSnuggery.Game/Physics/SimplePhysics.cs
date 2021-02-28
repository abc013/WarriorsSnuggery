using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Physics
{
	public sealed class SimplePhysics
	{
		public static readonly SimplePhysics Empty = new SimplePhysics(null, CPos.Zero, 0, Shape.NONE, 0, 0, 0);

		public readonly Shape Shape;
		public readonly int RadiusX;
		public readonly int RadiusY;// only for box and drawing
		public readonly int HeightRadius;

		public readonly CPos Offset;
		public readonly int HeightOffset;

		readonly PositionableObject positionable;

		public PhysicsSector[] Sectors = new PhysicsSector[0];

		public CPos Position => positionable.Position + Offset;
		public int Height => positionable.Height + HeightOffset;

		public bool IsEmpty => positionable == null || Shape == Shape.NONE;

		public SimplePhysics(PositionableObject positionable, CPos offset, int heightOffset, Shape shape, int radiusX, int radiusY, int heightradius)
		{
			this.positionable = positionable;
			Offset = offset;
			HeightOffset = heightOffset;
			Shape = shape;
			RadiusX = radiusX;
			RadiusY = radiusY;
			HeightRadius = heightradius;
		}

		public bool Intersects(SimplePhysics other)
		{
			if (IsEmpty || other.IsEmpty)
				return false;

			if (Math.Abs(other.Height - Height) >= other.HeightRadius + HeightRadius)
				return false;

			var diff = other.Position - Position;

			if (other.Shape != Shape.LINE_HORIZONTAL && Shape != Shape.LINE_HORIZONTAL && other.Shape != Shape.LINE_VERTICAL && Shape != Shape.LINE_VERTICAL)
			{
				if (Math.Abs(diff.X) >= other.RadiusX + RadiusX)
					return false;

				if (Math.Abs(diff.Y) >= other.RadiusX + RadiusX)
					return false;
			}

			if (Shape == Shape.CIRCLE && other.Shape == Shape.CIRCLE)
			{
				return diff.FlatDist <= RadiusX + other.RadiusX;
			}
			if (Shape == Shape.RECTANGLE && other.Shape == Shape.RECTANGLE)
			{
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
			return Shape switch
			{
				Shape.LINE_VERTICAL => new PhysicsLine[]
				{
					new PhysicsLine(Position - new CPos(RadiusX, 2 * RadiusY, 0), Position - new CPos(RadiusX, 0, 0))
				},
				Shape.LINE_HORIZONTAL => new PhysicsLine[]
				{
					new PhysicsLine(Position - new CPos(2 * RadiusX, RadiusY, 0), Position - new CPos(0, RadiusY, 0))
				},
				Shape.RECTANGLE => new PhysicsLine[]
				{
					new PhysicsLine(Position - new CPos(RadiusX, RadiusY, 0), Position + new CPos(-RadiusX, RadiusY, 0)),
					new PhysicsLine(Position - new CPos(RadiusX, RadiusY, 0), Position + new CPos(RadiusX, -RadiusY, 0)),
					new PhysicsLine(Position + new CPos(-RadiusX, RadiusY, 0), Position + new CPos(RadiusX, RadiusY, 0)),
					new PhysicsLine(Position + new CPos(RadiusX, -RadiusY, 0), Position + new CPos(RadiusX, RadiusY, 0))
				},
				_ => new PhysicsLine[0],
			};
		}

		public void RenderDebug()
		{
			switch (Shape)
			{
				case Shape.CIRCLE:
					ColorManager.DrawCircle(Position, RadiusX / 1024f, Color.Magenta);

					break;
				case Shape.RECTANGLE:
					ColorManager.DrawLineRect(Position, new CPos(RadiusX, RadiusY, 0), Color.Magenta);
					break;
				case Shape.LINE_HORIZONTAL:
					ColorManager.DrawLine(Position - new CPos(2 * RadiusX, RadiusY, 0), Position + new CPos(0, -RadiusY, 0), Color.Magenta);
					break;
				case Shape.LINE_VERTICAL:
					ColorManager.DrawLine(Position - new CPos(RadiusX, 2 * RadiusY, 0), Position + new CPos(-RadiusX, 0, 0), Color.Magenta);
					break;
			}
		}

		public void RemoveSectors()
		{
			if (IsEmpty)
				return;

			foreach (var sector in Sectors)
				sector.Remove(this);
		}
	}
}
