/*
 * User: Andreas
 * Date: 11.10.2017
 * 
 */

namespace WarriorsSnuggery
{
	public struct Vector
	{
		public static readonly Vector Zero = new Vector();

		public readonly float X;
		public readonly float Y;
		public readonly float Z;
		public readonly float W;

		public Vector(float x, float y, float z, float w = 0.0f)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public static Vector operator +(Vector lhs, Vector rhs) { return new Vector(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z, lhs.W + rhs.W); }

		public static Vector operator -(Vector lhs, Vector rhs) { return new Vector(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z, lhs.W - rhs.W); }

		public static Vector operator *(Vector lhs, Vector rhs) { return new Vector(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z, lhs.W * rhs.W); }

		public static Vector operator /(Vector lhs, Vector rhs) { return new Vector(lhs.X / rhs.X, lhs.Y / rhs.Y, lhs.Z / rhs.Z, lhs.W / rhs.W); }

		public static Vector operator -(Vector vec) { return new Vector(-vec.X, -vec.Y, -vec.Z, -vec.W); }

		public static implicit operator Vector(OpenTK.Vector4 vec)
		{
			return new Vector(vec.X, vec.Y, vec.Z, vec.W);
		}

		public static implicit operator OpenTK.Vector4(Vector vec)
		{
			return new OpenTK.Vector4(vec.X, vec.Y, vec.Z, vec.W);
		}

		public static bool operator ==(Vector lhs, Vector rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z && lhs.W == rhs.W; }

		public static bool operator !=(Vector lhs, Vector rhs) { return !(lhs == rhs); }

		public bool Equals(Vector pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is Vector && Equals((Vector)obj); }

		public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode(); }

		public override string ToString() { return X + "," + Y + "," + Z + "," + W; }

		public CPos ToCPos()
		{
			return new CPos((int)(X * 1024), (int)(Y * 1024), (int)(Z * 1024));
		}
	}
}
