using System;
using WarriorsSnuggery.Graphics;
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

			var diff = other.Position - Position;

			if (other.Type.Shape != Shape.LINE_HORIZONTAL && Type.Shape != Shape.LINE_HORIZONTAL && other.Type.Shape != Shape.LINE_VERTICAL && Type.Shape != Shape.LINE_VERTICAL)
			{
				if (Math.Abs(diff.X) >= other.Type.RadiusX + Type.RadiusX)
					return false;

				if (Math.Abs(diff.Y) >= other.Type.RadiusX + Type.RadiusX)
					return false;
			}

			if (Type.Shape == Shape.CIRCLE && other.Type.Shape == Shape.CIRCLE)
			{
				return diff.FlatDist <= Type.RadiusX + other.Type.RadiusX;
			}
			if (Type.Shape == Shape.RECTANGLE && other.Type.Shape == Shape.RECTANGLE)
			{
				var scaleX = Type.RadiusX + other.Type.RadiusX;
				var scaleY = Type.RadiusY + other.Type.RadiusY;
				return Math.Abs(diff.X) < scaleX && Math.Abs(diff.Y) < scaleY;
			}

			// CIRCLE <-> BOX
			if (Type.Shape == Shape.CIRCLE && other.Type.Shape == Shape.RECTANGLE || Type.Shape == Shape.RECTANGLE && other.Type.Shape == Shape.CIRCLE)
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
			if (Type.Shape == Shape.CIRCLE && other.Type.Shape == Shape.LINE_VERTICAL || Type.Shape == Shape.LINE_VERTICAL && other.Type.Shape == Shape.CIRCLE)
			{
				var circle = Type.Shape == Shape.CIRCLE ? this : other;
				var line = circle == this ? other : this;

				if (Math.Abs(circle.Position.Y - line.Position.Y + 512) > circle.Type.RadiusX + line.Type.RadiusX) return false;

				return Math.Abs(circle.Position.X - line.Position.X + 512) <= circle.Type.RadiusX;
			}

			if (Type.Shape == Shape.CIRCLE && other.Type.Shape == Shape.LINE_HORIZONTAL || Type.Shape == Shape.LINE_HORIZONTAL && other.Type.Shape == Shape.CIRCLE)
			{
				var circle = Type.Shape == Shape.CIRCLE ? this : other;
				var line = circle == this ? other : this;

				if (Math.Abs(circle.Position.X - line.Position.X + 512) > circle.Type.RadiusX + line.Type.RadiusX) return false;

				return Math.Abs(circle.Position.Y - line.Position.Y + 512) <= circle.Type.RadiusX;
			}

			// BOX <-> LINE
			if (Type.Shape == Shape.RECTANGLE && other.Type.Shape == Shape.LINE_HORIZONTAL || Type.Shape == Shape.LINE_HORIZONTAL && other.Type.Shape == Shape.RECTANGLE)
			{
				var box = Type.Shape == Shape.RECTANGLE ? this : other;
				var line = box == this ? other : this;

				if (Math.Abs(box.Position.Y - line.Position.Y + 512) > line.Type.RadiusX) return false;
				if (Math.Abs(box.Position.X - line.Position.X + 512) > (box.Type.RadiusY + line.Type.RadiusX)) return false;

				return true;
			}
			if (Type.Shape == Shape.RECTANGLE && other.Type.Shape == Shape.LINE_VERTICAL || Type.Shape == Shape.LINE_VERTICAL && other.Type.Shape == Shape.RECTANGLE)
			{
				var box = Type.Shape == Shape.RECTANGLE ? this : other;
				var line = box == this ? other : this;

				if (Math.Abs(box.Position.X - line.Position.X + 512) > line.Type.RadiusX) return false;
				if (Math.Abs(box.Position.Y - line.Position.Y + 512) > (box.Type.RadiusY + line.Type.RadiusX)) return false;

				return true;
			}

			return false;
		}

		public PhysicsLine[] GetLines()
		{
			if (IsEmpty)
				return new PhysicsLine[0];

			return Type.Shape switch
			{
				Shape.LINE_VERTICAL => new PhysicsLine[]
				{
					new PhysicsLine(Position - new CPos(Type.RadiusX, 2 * Type.RadiusY, 0), Position - new CPos(Type.RadiusX, 0, 0))
				},
				Shape.LINE_HORIZONTAL => new PhysicsLine[]
				{
					new PhysicsLine(Position - new CPos(2 * Type.RadiusX, Type.RadiusY, 0), Position - new CPos(0, Type.RadiusY, 0))
				},
				Shape.RECTANGLE => new PhysicsLine[]
				{
					new PhysicsLine(Position - new CPos(Type.RadiusX,  Type.RadiusY, 0), Position + new CPos(-Type.RadiusX, Type.RadiusY, 0)),
					new PhysicsLine(Position - new CPos(Type.RadiusX,  Type.RadiusY, 0), Position + new CPos(Type.RadiusX, -Type.RadiusY, 0)),
					new PhysicsLine(Position + new CPos(-Type.RadiusX, Type.RadiusY, 0), Position + new CPos(Type.RadiusX,  Type.RadiusY, 0)),
					new PhysicsLine(Position + new CPos(Type.RadiusX, -Type.RadiusY, 0), Position + new CPos(Type.RadiusX,  Type.RadiusY, 0))
				},
				_ => new PhysicsLine[0],
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
