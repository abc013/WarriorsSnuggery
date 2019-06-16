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
	}

	public static class GLPos
	{
		public const float PixelMultiplier = 1f / 24f;

		public static Vector FromPixel(int pxX, int pxY)
		{
			var x = (pxX / (float) WindowInfo.Width) * Camera.DefaultZoom * WindowInfo.Ratio;
			var y = (pxY / (float) WindowInfo.Height) * Camera.DefaultZoom;
			return new Vector(x,-y,0,0);
		}

		public static Vector FromScreen(int pxX, int pxY)
		{
			pxX -= WindowInfo.Width / 2;
			pxY -= WindowInfo.Height / 2;
			var x = (pxX / (float) WindowInfo.Width) * WindowInfo.Ratio;
			var y = (pxY / (float) WindowInfo.Height);
			return new Vector(x,-y,0,0);
		}

		public static CPos ToCPos(Vector pos)
		{
			return new CPos((int) (pos.X * 1024), (int) (-pos.Y * 1024), (int) (-pos.Z * 1024));
		}

		public static Vector ToVector(CPos pos)
		{
			return ToVector(pos.X / 1024f, pos.Y / 1024f, pos.Z / 1024f);
		}

		public static Vector ToVector(WPos pos)
		{
			return ToVector(pos.X, pos.Y, pos.Z);
		}

		public static Vector ToVector(MPos pos)
		{
			return ToVector(pos.X, pos.Y, 0f);
		}

		static Vector ToVector(float x, float y, float z)
		{
			return new Vector(x, -y, -z, 1.0f); //TODO fix this mess?
		}
	}
}
