using System;

namespace WarriorsSnuggery.Physics
{
	public static class Collision
	{
		public static bool CheckCollision(SimplePhysics a, SimplePhysics b, bool selfIntersection = false)
		{
			if (a == b)
				return selfIntersection;

			if (a.IsEmpty || b.IsEmpty)
				return false;

			if (Math.Abs(a.Height - b.Height) >= a.Boundaries.Z + b.Boundaries.Z)
				return false;

			bool areShapes(Shape sa, Shape sb) => a.Shape == sa && b.Shape == sb || a.Shape == sb && b.Shape == sa;

			if (a.Shape != Shape.LINE_HORIZONTAL && b.Shape != Shape.LINE_HORIZONTAL && a.Shape != Shape.LINE_VERTICAL && b.Shape != Shape.LINE_VERTICAL)
			{
				var diff = a.Position - b.Position;

				if (Math.Abs(diff.X) >= a.Boundaries.X + b.Boundaries.X)
					return false;

				if (Math.Abs(diff.Y) >= a.Boundaries.X + b.Boundaries.Y)
					return false;
			}

			if (a.Shape == Shape.CIRCLE && b.Shape == Shape.CIRCLE)
			{
				return checkCircleCollision(a, b);
			}

			if (a.Shape == Shape.RECTANGLE && b.Shape == Shape.RECTANGLE)
			{
				return checkBoxCollision(a, b);
			}

			if (areShapes(Shape.CIRCLE, Shape.RECTANGLE))
			{
				var circle = a.Shape == Shape.CIRCLE ? a : b;
				var box = circle == a ? b : a;

				return checkCircleBoxIntersection(circle, box);
			}

			if (areShapes(Shape.CIRCLE, Shape.LINE_HORIZONTAL) || areShapes(Shape.CIRCLE, Shape.LINE_VERTICAL))
			{
				var circle = a.Shape == Shape.CIRCLE ? a : b;
				var line = circle == a ? b : a;

				return checkLineCircleIntersection(line, circle);
			}

			if (areShapes(Shape.RECTANGLE, Shape.LINE_HORIZONTAL) || areShapes(Shape.RECTANGLE, Shape.LINE_VERTICAL))
			{
				var box = a.Shape == Shape.RECTANGLE ? a : b;
				var line = box == a ? b : a;

				return checkLineBoxIntersection(line, box);
			}

			return false;
		}

		static bool checkBoxCollision(SimplePhysics a, SimplePhysics b)
		{
			var diff = a.Position - b.Position;

			var scaleX = a.Boundaries.X + b.Boundaries.X;
			var scaleY = a.Boundaries.Y + b.Boundaries.Y;
			return Math.Abs(diff.X) < scaleX && Math.Abs(diff.Y) < scaleY;
		}

		static bool checkCircleCollision(SimplePhysics a, SimplePhysics b)
		{
			return (a.Position - b.Position).FlatDist <= a.Boundaries.X + b.Boundaries.X;
		}

		static bool checkCircleBoxIntersection(SimplePhysics circle, SimplePhysics box)
		{
			var pos = circle.Position - box.Position;
			pos = new CPos(Math.Abs(pos.X), Math.Abs(pos.Y), Math.Abs(pos.Z));

			if (pos.X > (box.Boundaries.X + circle.Boundaries.X)) return false;
			if (pos.Y > (box.Boundaries.Y + circle.Boundaries.Y)) return false;

			if (pos.X <= box.Boundaries.X) return true;
			if (pos.Y <= box.Boundaries.Y) return true;

			var corner = (pos.X - box.Boundaries.X) * (pos.X - box.Boundaries.X) + (pos.Y - box.Boundaries.Y) * (pos.Y - box.Boundaries.Y);

			return corner <= (circle.Boundaries.Y * circle.Boundaries.Y);
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
	}
}
