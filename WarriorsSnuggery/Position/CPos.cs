using System;

namespace WarriorsSnuggery
{
	public struct CPos
	{
		public static readonly CPos Zero = new CPos();

		public readonly int X;
		public readonly int Y;
		public readonly int Z;

		public CPos(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static CPos operator +(CPos lhs, CPos rhs) { return new CPos(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z); }

		public static CPos operator -(CPos lhs, CPos rhs) { return new CPos(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z); }

		public static CPos operator *(CPos lhs, CPos rhs) { return new CPos(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z); }

		public static CPos operator /(CPos lhs, CPos rhs) { return new CPos(lhs.X / rhs.X, lhs.Y / rhs.Y, lhs.Z / rhs.Z); }

		public static CPos operator -(CPos pos) { return new CPos(-pos.X, -pos.Y, -pos.Z); }

		public static bool operator ==(CPos lhs, CPos rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z; }

		public static bool operator !=(CPos lhs, CPos rhs) { return !(lhs == rhs); }

		public bool Equals(CPos pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is CPos && Equals((CPos)obj); }

		public override int GetHashCode() { return X ^ Y ^ Z; }

		public override string ToString() { return X + "," + Y + "," + Z; }

		/// <summary>
		/// Only uses X and Y coordinates
		/// </summary>
		public float Dist(CPos pos)
		{
			var x = (double)X - pos.X;
			var y = (double)Y - pos.Y;
			return (float)Math.Sqrt(x * x + y * y);
		}

		/// <summary>
		/// Only uses X and Y coordinates
		/// </summary>
		public float Angle(CPos pos)
		{
			var diff = pos - this;
			var diffX = -diff.X;
			var diffY = diff.Y;
			float angle = (float)-Math.Atan2(diffY, diffX);

			if (angle < 0f)
				angle += (float)(2 * Math.PI);

			return angle;
		}

		public Vector ToVector()
		{
			return new Vector(X / 1024f, -Y / 1024f, Z / 1024f, 1f);
		}

		public WPos ToWPos()
		{
			return new WPos(roundCorrect(X), roundCorrect(Y), roundCorrect(Z));
		}

		public MPos ToMPos()
		{
			return new MPos(roundCorrect(X), roundCorrect(Y));
		}

		int roundCorrect(int value)
		{
			var ans = value / 1024;

			if ((value & (1024 - 1)) > 512)
				return ans + Math.Sign(value);

			return ans;
		}
	}
}
