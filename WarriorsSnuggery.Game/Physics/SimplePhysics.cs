using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps.Layers;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Physics
{
	public sealed class SimplePhysics
	{
		public static readonly SimplePhysics Empty = new SimplePhysics(null, null);

		public readonly SimplePhysicsType Type;

		readonly PositionableObject positionable;

		public PhysicsSector[] Sectors = new PhysicsSector[0];

		public CPos Position => positionable.Position + Type.Offset;
		public int Height => positionable.Height + Type.HeightOffset;

		public bool IsEmpty => positionable == null || Type.Shape == Shape.NONE;

		public SimplePhysics(PositionableObject positionable, SimplePhysicsType type)
		{
			this.positionable = positionable;
			Type = type;
		}

		public bool Intersects(SimplePhysics other)
		{
			if (IsEmpty || other.IsEmpty)
				return false;

			if (Math.Abs(other.Height - Height) >= other.Type.HeightRadius + Type.HeightRadius)
				return false;

			bool areShapes(Shape a, Shape b) => other.Type.Shape == a && Type.Shape == b || other.Type.Shape == b && Type.Shape == a;

			if (other.Type.Shape != Shape.LINE_HORIZONTAL && Type.Shape != Shape.LINE_HORIZONTAL && other.Type.Shape != Shape.LINE_VERTICAL && Type.Shape != Shape.LINE_VERTICAL)
			{
				var diff = other.Position - Position;

				if (Math.Abs(diff.X) >= other.Type.RadiusX + Type.RadiusX)
					return false;

				if (Math.Abs(diff.Y) >= other.Type.RadiusX + Type.RadiusX)
					return false;
			}

			if (Type.Shape == Shape.CIRCLE && other.Type.Shape == Shape.CIRCLE)
			{
				return (other.Position - Position).FlatDist <= Type.RadiusX + other.Type.RadiusX;
			}

			if (Type.Shape == Shape.RECTANGLE && other.Type.Shape == Shape.RECTANGLE)
			{
				var diff = other.Position - Position;

				var scaleX = Type.RadiusX + other.Type.RadiusX;
				var scaleY = Type.RadiusY + other.Type.RadiusY;
				return Math.Abs(diff.X) < scaleX && Math.Abs(diff.Y) < scaleY;
			}

			if (areShapes(Shape.CIRCLE, Shape.RECTANGLE))
			{
				var circle = Type.Shape == Shape.CIRCLE ? this : other;
				var box = circle == this ? other : this;
				var pos = circle.Position - box.Position;
				pos = new CPos(Math.Abs(pos.X), Math.Abs(pos.Y), Math.Abs(pos.Z));

				if (pos.X > (box.Type.RadiusX + circle.Type.RadiusX)) return false;
				if (pos.Y > (box.Type.RadiusY + circle.Type.RadiusY)) return false;

				if (pos.X <= box.Type.RadiusX) return true;
				if (pos.Y <= box.Type.RadiusY) return true;

				// Pythagorean theorem: We calculate X and Y in order to get the circle distance; Added error margin "box.Radius/8".
				var corner = (pos.X - box.Type.RadiusX) ^ 2 + (pos.Y - box.Type.RadiusY) ^ 2 - box.Type.RadiusX / 8;

				return corner <= (circle.Type.RadiusX ^ 2);
			}

			// CIRCLE <-> LINE
			if (areShapes(Shape.CIRCLE, Shape.LINE_HORIZONTAL) || areShapes(Shape.CIRCLE, Shape.LINE_VERTICAL))
			{
				var circle = Type.Shape == Shape.CIRCLE ? this : other;
				var line = circle == this ? other : this;

				return checkLineCircleIntersection(line, circle);
			}

			// BOX <-> LINE
			if (areShapes(Shape.RECTANGLE, Shape.LINE_HORIZONTAL) || areShapes(Shape.RECTANGLE, Shape.LINE_VERTICAL))
			{
				var box = Type.Shape == Shape.RECTANGLE ? this : other;
				var line = box == this ? other : this;

				return checkLineBoxIntersection(line, box);
			}

			return false;
		}

		static bool checkLineCircleIntersection(SimplePhysics line, SimplePhysics circle)
		{
			if (line.Type.Shape == Shape.LINE_HORIZONTAL)
			{
				if (Math.Abs(circle.Position.X - line.Position.X + 512) > circle.Type.RadiusX + line.Type.RadiusX) return false;
				if (Math.Abs(circle.Position.Y - line.Position.Y + 512) > circle.Type.RadiusY) return false;
			}
			else
			{
				if (Math.Abs(circle.Position.X - line.Position.X + 512) > circle.Type.RadiusX) return false;
				if (Math.Abs(circle.Position.Y - line.Position.Y + 512) > circle.Type.RadiusY + line.Type.RadiusY) return false;
			}

			return true;
		}

		static bool checkLineBoxIntersection(SimplePhysics line, SimplePhysics box)
		{
			if (line.Type.Shape == Shape.LINE_HORIZONTAL)
			{
				if (Math.Abs(box.Position.X - line.Position.X + 512) > (box.Type.RadiusX + line.Type.RadiusX)) return false;
				if (Math.Abs(box.Position.Y - line.Position.Y + 512) > box.Type.RadiusY) return false;
			}
			else
			{
				if (Math.Abs(box.Position.X - line.Position.X + 512) > box.Type.RadiusX) return false;
				if (Math.Abs(box.Position.Y - line.Position.Y + 512) > (box.Type.RadiusY + line.Type.RadiusY)) return false;
			}

			return true;
		}

		public (CPos start, CPos end)[] GetLines()
		{
			return Type.Shape switch
			{
				Shape.LINE_VERTICAL => new (CPos, CPos)[]
				{
					(Position - new CPos(Type.RadiusX, 2 * Type.RadiusY, 0), Position - new CPos(Type.RadiusX, 0, 0))
				},
				Shape.LINE_HORIZONTAL => new (CPos, CPos)[]
				{
					(Position - new CPos(2 * Type.RadiusX, Type.RadiusY, 0), Position - new CPos(0, Type.RadiusY, 0))
				},
				Shape.RECTANGLE => new (CPos, CPos)[]
				{
					(Position - new CPos(Type.RadiusX,  Type.RadiusY, 0), Position + new CPos(-Type.RadiusX, Type.RadiusY, 0)),
					(Position - new CPos(Type.RadiusX,  Type.RadiusY, 0), Position + new CPos(Type.RadiusX, -Type.RadiusY, 0)),
					(Position + new CPos(-Type.RadiusX, Type.RadiusY, 0), Position + new CPos(Type.RadiusX,  Type.RadiusY, 0)),
					(Position + new CPos(Type.RadiusX, -Type.RadiusY, 0), Position + new CPos(Type.RadiusX,  Type.RadiusY, 0))
				},
				_ => new (CPos, CPos)[0],
			};
		}

		public void RenderDebug()
		{
			if (IsEmpty)
				return;

			switch (Type.Shape)
			{
				case Shape.CIRCLE:
					ColorManager.DrawCircle(Position, Type.RadiusX / 1024f, Color.Magenta);
					break;
				case Shape.RECTANGLE:
					ColorManager.DrawLineQuad(Position, new CPos(Type.RadiusX, Type.RadiusY, 0), Color.Magenta);
					break;
				case Shape.LINE_HORIZONTAL:
					ColorManager.DrawLine(Position - new CPos(2 * Type.RadiusX, Type.RadiusY, 0), Position + new CPos(0, -Type.RadiusY, 0), Color.Magenta);
					break;
				case Shape.LINE_VERTICAL:
					ColorManager.DrawLine(Position - new CPos(Type.RadiusX, 2 * Type.RadiusY, 0), Position + new CPos(-Type.RadiusX, 0, 0), Color.Magenta);
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
