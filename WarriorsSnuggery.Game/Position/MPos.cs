using System;

namespace WarriorsSnuggery
{
	public struct MPos
	{
		public static readonly MPos Zero = new MPos();

		public readonly int X;
		public readonly int Y;

		public float SquaredDist => X * (long)X + Y * (long)Y;
		public float Dist => (float)Math.Sqrt(SquaredDist);

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
		public override bool Equals(object obj) { return obj is MPos pos && Equals(pos); }

		public override int GetHashCode() { return X ^ Y; }

		public override string ToString() { return X + "," + Y; }

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

		public bool IsInRange(MPos minimum, MPos maximum)
		{
			if (X < minimum.X) return false;
			if (Y < minimum.Y) return false;
			if (X > maximum.X) return false;
			if (Y > maximum.Y) return false;

			return true;
		}

		public CPos ToCPos()
		{
			return new CPos(X * 1024, Y * 1024, 0);
		}
	}
}
