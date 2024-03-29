using OpenTK.Mathematics;
using System;

namespace WarriorsSnuggery
{
	public readonly struct Vector
	{
		public static readonly Vector Zero = new Vector();

		public float Dist => (float)Math.Sqrt((double)X * X + (double)Y * Y + (double)Z * Z);

		public readonly float X;
		public readonly float Y;
		public readonly float Z;

		public Vector(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static Vector operator +(in Vector lhs, in Vector rhs) { return new Vector(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z); }
		public static Vector operator -(in Vector lhs, in Vector rhs) { return new Vector(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z); }
		public static Vector operator *(in Vector lhs, in Vector rhs) { return new Vector(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z); }
		public static Vector operator /(in Vector lhs, in Vector rhs) { return new Vector(lhs.X / rhs.X, lhs.Y / rhs.Y, lhs.Z / rhs.Z); }
		public static Vector operator -(in Vector vec) { return new Vector(-vec.X, -vec.Y, -vec.Z); }

		public static bool operator ==(in Vector lhs, in Vector rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z; }
		public static bool operator !=(in Vector lhs, in Vector rhs) { return !(lhs == rhs); }

		public static implicit operator Vector(in Vector3 vec)
		{
			return new Vector(vec.X, vec.Y, vec.Z);
		}

		public static implicit operator Vector3(in Vector vec)
		{
			return new Vector3(vec.X, vec.Y, vec.Z);
		}

		public bool Equals(in Vector pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is Vector vector && Equals(vector); }

		public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode(); }

		public override string ToString() { return X + ", " + Y + ", " + Z; }

		public CPos ToCPos()
		{
			return new CPos((int)(X * Constants.TileSize), (int)(Y * Constants.TileSize), (int)(Z * Constants.TileSize));
		}

		public static Vector FromFlatAngle(float angle, float magnitude)
		{
			var x = MathF.Cos(angle) * magnitude;
			var y = MathF.Sin(angle) * magnitude;

			return new Vector(x, y, 0);
		}
	}
}
