using System;

namespace WarriorsSnuggery
{
	public readonly struct MPos
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

		public static MPos operator +(in MPos lhs, in MPos rhs) { return new MPos(lhs.X + rhs.X, lhs.Y + rhs.Y); }

		public static MPos operator -(in MPos lhs, in MPos rhs) { return new MPos(lhs.X - rhs.X, lhs.Y - rhs.Y); }

		public static MPos operator *(in MPos lhs, in MPos rhs) { return new MPos(lhs.X * rhs.X, lhs.Y * rhs.Y); }
		public static MPos operator *(in MPos lhs, int rhs) { return new MPos(lhs.X * rhs, lhs.Y * rhs); }
		public static MPos operator *(int lhs, in MPos rhs) { return rhs * lhs; }

		public static MPos operator /(in MPos lhs, in MPos rhs) { return new MPos(lhs.X / rhs.X, lhs.Y / rhs.Y); }
		public static MPos operator /(in MPos lhs, int rhs) { return new MPos(lhs.X / rhs, lhs.Y / rhs); }

		public static bool operator ==(in MPos lhs, in MPos rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y; }

		public static bool operator !=(in MPos lhs, in MPos rhs) { return !(lhs == rhs); }

		public bool Equals(in MPos pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is MPos pos && Equals(pos); }

		public override int GetHashCode() { return X ^ Y; }

		public override string ToString() { return X + ", " + Y; }

		public float AngleTo(MPos pos)
		{
			var diff = pos - this;
			return Angle.FromVector(diff.X, diff.Y);
		}

		public bool InRange(MPos inclusiveMinimum, MPos exclusiveMaximum)
		{
			if (X < inclusiveMinimum.X) return false;
			if (Y < inclusiveMinimum.Y) return false;
			if (X >= exclusiveMaximum.X) return false;
			if (Y >= exclusiveMaximum.Y) return false;

			return true;
		}

		public CPos ToCPos()
		{
			return new CPos(X * Constants.TileSize, Y * Constants.TileSize, 0);
		}
	}
}
