
namespace WarriorsSnuggery
{
	public struct Vector
	{
		public static readonly Vector Zero = new Vector();

		public readonly float X;
		public readonly float Y;
		public readonly float Z;

		public Vector(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static Vector operator +(Vector lhs, Vector rhs) { return new Vector(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z); }

		public static Vector operator -(Vector lhs, Vector rhs) { return new Vector(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z); }

		public static Vector operator *(Vector lhs, Vector rhs) { return new Vector(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z); }

		public static Vector operator /(Vector lhs, Vector rhs) { return new Vector(lhs.X / rhs.X, lhs.Y / rhs.Y, lhs.Z / rhs.Z); }

		public static Vector operator -(Vector vec) { return new Vector(-vec.X, -vec.Y, -vec.Z); }

		public static implicit operator Vector(OpenTK.Vector4 vec)
		{
			return new Vector(vec.X, vec.Y, vec.Z);
		}

		public static implicit operator OpenTK.Vector4(Vector vec)
		{
			return new OpenTK.Vector4(vec.X, vec.Y, vec.Z, 1f);
		}

		public static bool operator ==(Vector lhs, Vector rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z; }

		public static bool operator !=(Vector lhs, Vector rhs) { return !(lhs == rhs); }

		public bool Equals(Vector pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is Vector && Equals((Vector)obj); }

		public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode(); }

		public override string ToString() { return X + "," + Y + "," + Z; }

		public CPos ToCPos()
		{
			return new CPos((int)(X * 1024), (int)(Y * 1024), (int)(Z * 1024));
		}
	}
}
