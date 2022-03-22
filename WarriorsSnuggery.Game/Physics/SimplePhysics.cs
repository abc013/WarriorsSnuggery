using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps.Layers;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Physics
{
	public sealed class SimplePhysics
	{
		public static readonly SimplePhysics Empty = new SimplePhysics(null, null);

		readonly SimplePhysicsType type;

		readonly PositionableObject positionable;

		public PhysicsSector[] Sectors = new PhysicsSector[0];

		public CPos Position => positionable.Position + type.Offset;
		public int Height => positionable.Height + type.HeightOffset;

		public bool IsEmpty => positionable == null || type.Shape == Shape.NONE;

		public (int X, int Y, int Z) Boundaries => (type.RadiusX, type.RadiusY, type.HeightRadius);
		public Shape Shape => type.Shape;

		public SimplePhysics(PositionableObject positionable, SimplePhysicsType type)
		{
			this.positionable = positionable;
			this.type = type;
		}

		public bool Intersects(SimplePhysics other)
		{
			if (IsEmpty || other.IsEmpty)
				return false;

			if (Math.Abs(other.Height - Height) >= other.Boundaries.Z + Boundaries.Z)
				return false;

			bool areShapes(Shape a, Shape b) => other.Shape == a && Shape == b || other.Shape == b && Shape == a;

			if (other.Shape != Shape.LINE_HORIZONTAL && Shape != Shape.LINE_HORIZONTAL && other.Shape != Shape.LINE_VERTICAL && Shape != Shape.LINE_VERTICAL)
			{
				var diff = other.Position - Position;

				if (Math.Abs(diff.X) >= other.Boundaries.X + Boundaries.X)
					return false;

				if (Math.Abs(diff.Y) >= other.Boundaries.X + Boundaries.Y)
					return false;
			}

			if (Shape == Shape.CIRCLE && other.Shape == Shape.CIRCLE)
			{
				return (other.Position - Position).FlatDist <= Boundaries.X + other.Boundaries.X;
			}

			if (Shape == Shape.RECTANGLE && other.Shape == Shape.RECTANGLE)
			{
				var diff = other.Position - Position;

				var scaleX = Boundaries.X + other.Boundaries.X;
				var scaleY = Boundaries.Y + other.Boundaries.Y;
				return Math.Abs(diff.X) < scaleX && Math.Abs(diff.Y) < scaleY;
			}

			if (areShapes(Shape.CIRCLE, Shape.RECTANGLE))
			{
				var circle = Shape == Shape.CIRCLE ? this : other;
				var box = circle == this ? other : this;
				var pos = circle.Position - box.Position;
				pos = new CPos(Math.Abs(pos.X), Math.Abs(pos.Y), Math.Abs(pos.Z));

				if (pos.X > (box.Boundaries.X + circle.Boundaries.X)) return false;
				if (pos.Y > (box.Boundaries.Y + circle.Boundaries.Y)) return false;

				if (pos.X <= box.Boundaries.X) return true;
				if (pos.Y <= box.Boundaries.Y) return true;

				var corner = (pos.X - box.Boundaries.X) * (pos.X - box.Boundaries.X) + (pos.Y - box.Boundaries.Y) * (pos.Y - box.Boundaries.Y);

				return corner <= (circle.Boundaries.Y * circle.Boundaries.Y);
			}

			// CIRCLE <-> LINE
			if (areShapes(Shape.CIRCLE, Shape.LINE_HORIZONTAL) || areShapes(Shape.CIRCLE, Shape.LINE_VERTICAL))
			{
				var circle = Shape == Shape.CIRCLE ? this : other;
				var line = circle == this ? other : this;

				return checkLineCircleIntersection(line, circle);
			}

			// BOX <-> LINE
			if (areShapes(Shape.RECTANGLE, Shape.LINE_HORIZONTAL) || areShapes(Shape.RECTANGLE, Shape.LINE_VERTICAL))
			{
				var box = Shape == Shape.RECTANGLE ? this : other;
				var line = box == this ? other : this;

				return checkLineBoxIntersection(line, box);
			}

			return false;
		}

		static bool checkLineCircleIntersection(SimplePhysics line, SimplePhysics circle)
		{
			if (line.Shape == Shape.LINE_HORIZONTAL)
			{
				if (Math.Abs(circle.Position.X - line.Position.X + 512) > circle.Boundaries.X + line.Boundaries.X) return false;
				if (Math.Abs(circle.Position.Y - line.Position.Y + 512) > circle.Boundaries.Y) return false;
			}
			else
			{
				if (Math.Abs(circle.Position.X - line.Position.X + 512) > circle.Boundaries.X) return false;
				if (Math.Abs(circle.Position.Y - line.Position.Y + 512) > circle.Boundaries.Y + line.Boundaries.Y) return false;
			}

			return true;
		}

		static bool checkLineBoxIntersection(SimplePhysics line, SimplePhysics box)
		{
			if (line.Shape == Shape.LINE_HORIZONTAL)
			{
				if (Math.Abs(box.Position.X - line.Position.X + 512) > (box.Boundaries.X + line.Boundaries.X)) return false;
				if (Math.Abs(box.Position.Y - line.Position.Y + 512) > box.Boundaries.Y) return false;
			}
			else
			{
				if (Math.Abs(box.Position.X - line.Position.X + 512) > box.Boundaries.X) return false;
				if (Math.Abs(box.Position.Y - line.Position.Y + 512) > (box.Boundaries.Y + line.Boundaries.Y)) return false;
			}

			return true;
		}

		public (CPos start, CPos end)[] GetLines()
		{
			return Shape switch
			{
				Shape.LINE_VERTICAL => new (CPos, CPos)[]
				{
					(Position - new CPos(Boundaries.X, 2 * Boundaries.Y, 0), Position - new CPos(Boundaries.X, 0, 0))
				},
				Shape.LINE_HORIZONTAL => new (CPos, CPos)[]
				{
					(Position - new CPos(2 * Boundaries.X, Boundaries.Y, 0), Position - new CPos(0, Boundaries.Y, 0))
				},
				Shape.RECTANGLE => new (CPos, CPos)[]
				{
					(Position - new CPos(Boundaries.X,  Boundaries.Y, 0), Position + new CPos(-Boundaries.X, Boundaries.Y, 0)),
					(Position - new CPos(Boundaries.X,  Boundaries.Y, 0), Position + new CPos(Boundaries.X, -Boundaries.Y, 0)),
					(Position + new CPos(-Boundaries.X, Boundaries.Y, 0), Position + new CPos(Boundaries.X,  Boundaries.Y, 0)),
					(Position + new CPos(Boundaries.X, -Boundaries.Y, 0), Position + new CPos(Boundaries.X,  Boundaries.Y, 0))
				},
				_ => new (CPos, CPos)[0],
			};
		}

		public void RenderDebug()
		{
			if (IsEmpty)
				return;

			switch (Shape)
			{
				case Shape.CIRCLE:
					ColorManager.DrawCircle(Position, Boundaries.X / 1024f, Color.Magenta);
					break;
				case Shape.RECTANGLE:
					ColorManager.DrawLineQuad(Position, new CPos(Boundaries.X, Boundaries.Y, 0), Color.Magenta);
					break;
				case Shape.LINE_HORIZONTAL:
					ColorManager.DrawLine(Position - new CPos(2 * Boundaries.X, Boundaries.Y, 0), Position + new CPos(0, -Boundaries.Y, 0), Color.Magenta);
					break;
				case Shape.LINE_VERTICAL:
					ColorManager.DrawLine(Position - new CPos(Boundaries.X, 2 * Boundaries.Y, 0), Position + new CPos(-Boundaries.X, 0, 0), Color.Magenta);
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
