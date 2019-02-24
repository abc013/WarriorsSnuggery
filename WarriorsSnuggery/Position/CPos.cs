using System;

namespace WarriorsSnuggery
{
	public struct CPos
	{
		public static readonly CPos Zero = new CPos();

		public readonly int X, Y, Z;

		public CPos(int x, int y, int z) { X = x; Y = y; Z = z; }

		public static CPos operator +(CPos lhs, CPos rhs) { return new CPos(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z); }

		public static CPos operator -(CPos lhs, CPos rhs) { return new CPos(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z); }

		public static CPos operator *(CPos lhs, CPos rhs) { return new CPos(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z); }

		public static CPos operator /(CPos lhs, CPos rhs) { return new CPos(lhs.X / rhs.X, lhs.Y / rhs.Y, lhs.Z / rhs.Z); }

		public static bool operator ==(CPos lhs, CPos rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z; }

		public static bool operator !=(CPos lhs, CPos rhs) { return !(lhs == rhs); }

		public bool Equals(CPos pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is CPos && Equals((CPos) obj); }

		public override int GetHashCode() { return X ^ Y ^ Z; }

		public override string ToString() { return X + "," + Y + "," + Z; }

		public float GetDistToXY(CPos pos)
		{
			var x = (double) X - pos.X;
			var y = (double) Y - pos.Y;
			return (float) Math.Sqrt(x * x  + y * y);
		}

		public float GetAngleToXY(CPos pos)
		{
			var diff = pos - this;
			var diffX = -diff.X;
			var diffY = diff.Y;
			float angle = (float)-Math.Atan2(diffY, diffX);
			float degrees = (180 / (float)Math.PI) * angle;
			degrees = degrees > 0 ? degrees : degrees + 360f;
			return degrees;
		}

		public OpenTK.Vector4 ToAngle()
		{
			const float degree = 1 / 180f * (float) Math.PI;

			return new OpenTK.Vector4(X * degree, Y *degree, Z*degree, 0f);
		}

		public OpenTK.Vector4 ToVector()
		{
			return GLPos.ToVector(this);
		}

		public WPos ToWPos()
		{
			return new WPos(roundCorrect(X),roundCorrect(Y),roundCorrect(Z));
		}

		public MPos ToMPos()
		{
			return new MPos(roundCorrect(X),roundCorrect(Y));
		}

		int roundCorrect(int value)
		{
			return value % 1024 > 512 ? (int) Math.Ceiling(value / 1024f) : (int) Math.Floor(value / 1024f);
		}
	}
}
