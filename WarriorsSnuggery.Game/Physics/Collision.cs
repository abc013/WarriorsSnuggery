using System;

namespace WarriorsSnuggery.Physics
{
	public class Collision
	{
		public readonly float Angle;
		public readonly CPos Position;

		public Collision(float angle, CPos position)
		{
			Angle = angle;
			Position = position;
		}

		public static bool CheckCollision(SimplePhysics a, SimplePhysics b, out Collision collision)
		{
			collision = null;

			if (a == b)
				return false; // no self-intersection

			if (a.IsEmpty || b.IsEmpty)
				return false;

			if (Math.Abs(a.Position.Z - b.Position.Z) >= a.Boundaries.Z + b.Boundaries.Z)
				return false;

			// Collision between lines are not considered
			if (a.Shape == Shape.LINE && b.Shape == Shape.LINE)
				return false;

			var diff = a.Position - b.Position;

			if (Math.Abs(diff.X) >= a.Boundaries.X + b.Boundaries.X) return false;
			if (Math.Abs(diff.Y) >= a.Boundaries.X + b.Boundaries.Y) return false;

			if (a.Shape == Shape.CIRCLE && b.Shape == Shape.CIRCLE)
			{
				return checkCircleCollision(diff, a, b, out collision);
			}

			if (a.Shape == Shape.RECTANGLE && b.Shape == Shape.RECTANGLE)
			{
				return checkBoxCollision(diff, a, b);
			}

			bool areShapes(Shape sa, Shape sb) => a.Shape == sa && b.Shape == sb || a.Shape == sb && b.Shape == sa;

			if (areShapes(Shape.CIRCLE, Shape.RECTANGLE))
			{
				var circle = a.Shape == Shape.CIRCLE ? a : b;
				var box = circle == a ? b : a;

				return checkCircleBoxIntersection(diff, circle, box, out collision);
			}

			if (areShapes(Shape.CIRCLE, Shape.LINE))
			{
				var circle = a.Shape == Shape.CIRCLE ? a : b;
				var line = circle == a ? b : a;

				return checkLineCircleIntersection(diff, line, circle);
			}

			if (areShapes(Shape.RECTANGLE, Shape.LINE))
			{
				var box = a.Shape == Shape.RECTANGLE ? a : b;
				var line = box == a ? b : a;

				return checkLineBoxIntersection(diff, line, box);
			}

			return false;
		}

		static bool checkBoxCollision(CPos diff, SimplePhysics a, SimplePhysics b)
		{
			return Math.Abs(diff.X) < a.Boundaries.X + b.Boundaries.X && Math.Abs(diff.Y) < a.Boundaries.Y + b.Boundaries.Y;
		}

		static bool checkCircleCollision(CPos diff, SimplePhysics a, SimplePhysics b, out Collision collision)
		{
			collision = null;

			if (diff.FlatDist > a.Boundaries.X + b.Boundaries.X)
				return false;

			var angle = diff.FlatAngle;
			var position = b.Position + new CPos(diff.X / 2, diff.Y / 2, diff.Z / 2);
			collision = new Collision(angle, position);

			return true;
		}

		static bool checkCircleBoxIntersection(CPos diff, SimplePhysics circle, SimplePhysics box, out Collision collision)
		{
			collision = null;
			var pos = new CPos(Math.Abs(diff.X), Math.Abs(diff.Y), Math.Abs(diff.Z));

			if (pos.X > (box.Boundaries.X + circle.Boundaries.X)) return false;
			if (pos.Y > (box.Boundaries.Y + circle.Boundaries.Y)) return false;

			if (pos.X <= box.Boundaries.X)
			{
				var angle = new CPos(0, circle.Position.Y - box.Position.Y, 0).FlatAngle;
				var position = circle.Position + new CPos(0, circle.Position.Y - box.Position.Y, 0);
				collision = new Collision(angle, position);
				return true;
			}

			if (pos.Y <= box.Boundaries.Y)
			{
				var angle = new CPos(circle.Position.X - box.Position.X, 0, 0).FlatAngle;
				var position = circle.Position + new CPos(circle.Position.X - box.Position.X, 0, 0);
				collision = new Collision(angle, position);
				return true;
			}

			var corner = (pos.X - box.Boundaries.X) * (pos.X - box.Boundaries.X) + (pos.Y - box.Boundaries.Y) * (pos.Y - box.Boundaries.Y);

			if (corner <= (circle.Boundaries.Y * circle.Boundaries.Y))
			{
				var angle = (box.Position - circle.Position).FlatAngle;
				var position = circle.Position + (circle.Position - box.Position) / new CPos(2, 2, 2);
				collision = new Collision(angle, position);
				return true;
			}

			return false;
		}

		static bool checkLineCircleIntersection(CPos diff, SimplePhysics line, SimplePhysics circle)
		{
			if (line.Boundaries.Y == 0)
			{
				if (Math.Abs(diff.X) > circle.Boundaries.X + line.Boundaries.X) return false;
				if (Math.Abs(diff.Y) > circle.Boundaries.Y) return false;
			}
			else
			{
				if (Math.Abs(diff.X) > circle.Boundaries.X) return false;
				if (Math.Abs(diff.Y) > circle.Boundaries.Y + line.Boundaries.Y) return false;
			}

			return true;
		}

		static bool checkLineBoxIntersection(CPos diff, SimplePhysics line, SimplePhysics box)
		{
			if (line.Boundaries.Y == 0)
			{
				if (Math.Abs(diff.X) > (box.Boundaries.X + line.Boundaries.X)) return false;
				if (Math.Abs(diff.Y) > box.Boundaries.Y) return false;
			}
			else
			{
				if (Math.Abs(diff.X) > box.Boundaries.X) return false;
				if (Math.Abs(diff.Y) > (box.Boundaries.Y + line.Boundaries.Y)) return false;
			}

			return true;
		}
	}
}
