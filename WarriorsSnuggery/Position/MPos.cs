/*
 * User: Andreas
 * Date: 05.10.2017
 * 
 */
using System;

namespace WarriorsSnuggery
{
	public struct MPos
	{
		public static readonly MPos Zero = new MPos();

		public readonly int X;
		public readonly int Y;

		public MPos(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static MPos operator +(MPos lhs, MPos rhs) { return new MPos(lhs.X + rhs.X, lhs.Y + rhs.Y); }

		public static MPos operator -(MPos lhs, MPos rhs) { return new MPos(lhs.X - rhs.X, lhs.Y - rhs.Y); }

		public static MPos operator *(MPos lhs, MPos rhs) { return new MPos(lhs.X * rhs.X, lhs.Y * rhs.Y); }

		public static MPos operator /(MPos lhs, MPos rhs) { return new MPos(lhs.X / rhs.X, lhs.Y / rhs.Y); }

		public static bool operator ==(MPos lhs, MPos rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y; }

		public static bool operator !=(MPos lhs, MPos rhs) { return !(lhs == rhs); }

		public bool Equals(MPos pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is MPos && Equals((MPos)obj); }

		public override int GetHashCode() { return X ^ Y; }

		public override string ToString() { return X + "," + Y; }

		public float DistTo(MPos pos)
		{
			var x = (double)X - pos.X;
			var y = (double)Y - pos.Y;
			return (float)Math.Sqrt(x * x + y * y);
		}

		public float AngleTo(MPos pos)
		{
			var diff = pos - this;
			var diffX = -diff.X;
			var diffY = diff.Y;
			float angle = (float)-Math.Atan2(diffY, diffX);

			if (angle < 0f)
				angle += (float)(2 * Math.PI);

			return angle;
		}

		public bool IsInRange(MPos minimum, MPos range)
		{
			if (X < minimum.X) return false;
			if (Y < minimum.Y) return false;
			if (X > range.X) return false;
			if (Y > range.Y) return false;

			return true;
		}

		public WPos ToWPos()
		{
			return new WPos(X, Y, 0);
		}

		public CPos ToCPos()
		{
			return new CPos(X * 1024, Y * 1024, 0);
		}
	}
}
