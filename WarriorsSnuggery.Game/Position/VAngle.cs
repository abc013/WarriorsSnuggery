using OpenTK.Mathematics;

namespace WarriorsSnuggery
{
	public readonly struct VAngle
	{
		public static readonly VAngle Zero = new VAngle();

		public readonly float X;
		public readonly float Y;
		public readonly float Z;

		public VAngle(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public VAngle(int xDeg, int yDeg, int zDeg)
		{
			X = Angle.ToArc(xDeg);
			Y = Angle.ToArc(yDeg);
			Z = Angle.ToArc(zDeg);
		}

		public static VAngle operator +(VAngle lhs, VAngle rhs) { return new VAngle(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z); }

		public static VAngle operator -(VAngle lhs, VAngle rhs) { return new VAngle(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z); }

		public static VAngle operator *(VAngle lhs, VAngle rhs) { return new VAngle(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z); }

		public static VAngle operator /(VAngle lhs, VAngle rhs) { return new VAngle(lhs.X / rhs.X, lhs.Y / rhs.Y, lhs.Z / rhs.Z); }

		public static VAngle operator -(VAngle pos) { return new VAngle(-pos.X, -pos.Y, -pos.Z); }

		public static bool operator ==(VAngle lhs, VAngle rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z; }

		public static bool operator !=(VAngle lhs, VAngle rhs) { return !(lhs == rhs); }

		public bool Equals(VAngle pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is VAngle angle && Equals(angle); }

		public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode(); }

		public override string ToString() { return X + ", " + Y + ", " + Z; }

		public VAngle CastToAngleRange()
		{
			return new VAngle(Angle.Cast(X), Angle.Cast(Y), Angle.Cast(Z));
		}

		public CPos ToDegree()
		{
			return new CPos(Angle.ToDegree(X), Angle.ToDegree(Y), Angle.ToDegree(Z));
		}

		public static implicit operator Vector3(VAngle angle)
		{
			return new Vector3(angle.X, angle.Y, angle.Z);
		}
	}
}
