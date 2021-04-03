﻿using System;

namespace WarriorsSnuggery
{
	public readonly struct CPos
	{
		public static readonly CPos Zero = new CPos();

		public readonly int X;
		public readonly int Y;
		public readonly int Z;

		public float Dist => (float)Math.Sqrt(X * (double)X + Y * (double)Y + Z * (double)Z);

		public long SquaredFlatDist => X * (long)X + Y * (long)Y;
		public float FlatDist => MathF.Sqrt(SquaredFlatDist);

		public float FlatAngle => Angle.FromVector(X, Y);

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
		public override bool Equals(object obj) { return obj is CPos pos && Equals(pos); }

		public override int GetHashCode() { return X ^ Y ^ Z; }

		public override string ToString() { return X + ", " + Y + ", " + Z; }

		public Vector ToVector()
		{
			return new Vector(X / 1024f, -Y / 1024f, Z / 1024f);
		}

		public MPos ToMPos()
		{
			return new MPos(round(X), round(Y));
		}

		int round(int value)
		{
			var ans = value / 1024;

			if ((value & (1024 - 1)) > 512)
				return ans + Math.Sign(value);

			return ans;
		}

		public static CPos FromFlatAngle(float angle, float magnitude)
		{
			var x = (int)(MathF.Cos(angle) * magnitude);
			var y = (int)(MathF.Sin(angle) * magnitude);

			return new CPos(x, y, 0);
		}
	}
}
