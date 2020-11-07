using OpenTK.Mathematics;
using System;

namespace WarriorsSnuggery
{
	public struct VAngle
	{
		public const float MaxRange = (float)(2 * Math.PI);
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
			const float u = (float)(Math.PI / 180);
			X = u * xDeg;
			Y = u * yDeg;
			Z = u * zDeg;
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

		public override string ToString() { return X + "," + Y + "," + Z; }

		public VAngle CastToAngleRange()
		{
			//0 to 2 PI;
			var xDeg = cast(X);
			var yDeg = cast(Y);
			var zDeg = cast(Z);

			return new VAngle(xDeg, yDeg, zDeg);
		}

		float cast(float deg)
		{
			if (deg < 0 || deg > MaxRange)
			{
				deg %= MaxRange;

				if (deg < 0)
					deg += MaxRange;
			}
			return deg;
		}

		public CPos ToDegree()
		{
			const float u = (float)(180 / Math.PI);
			var x = (int)(u * X);
			var y = (int)(u * Y);
			var z = (int)(u * Z);

			return new CPos(x, y, z);
		}

		public static implicit operator Vector3(VAngle angle)
		{
			return new Vector3(angle.X, angle.Y, angle.Z);
		}
	}
}
