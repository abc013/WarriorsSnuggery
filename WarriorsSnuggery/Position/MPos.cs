/*
 * User: Andreas
 * Date: 05.10.2017
 * 
 */
using System;

namespace WarriorsSnuggery
{
	public struct MPos
	{
		public static readonly MPos Zero = new MPos();

		public readonly int X,Y;

		public MPos(int x, int y) { X = x; Y = y; }

		public static MPos operator +(MPos lhs, MPos rhs) { return new MPos(lhs.X + rhs.X, lhs.Y + rhs.Y); }

		public static MPos operator -(MPos lhs, MPos rhs) { return new MPos(lhs.X - rhs.X, lhs.Y - rhs.Y); }

		public static MPos operator *(MPos lhs, MPos rhs) { return new MPos(lhs.X * rhs.X, lhs.Y * rhs.Y); }

		public static MPos operator /(MPos lhs, MPos rhs) { return new MPos(lhs.X / rhs.X, lhs.Y / rhs.Y); }

		public static bool operator ==(MPos lhs, MPos rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y; }

		public static bool operator !=(MPos lhs, MPos rhs) { return !(lhs == rhs); }

		public bool Equals(MPos pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is MPos && Equals((MPos) obj); }

		public override int GetHashCode() { return X ^ Y; }

		public override string ToString() { return X + "," + Y; }

		public OpenTK.Vector4 ToVector()
		{
			return GLPos.ToVector(this);
		}

		public WPos ToWPos()
		{
			return new WPos(X, Y, 0);
		}

		public CPos ToCPos()
		{
			return new CPos(X * 1024, Y * 1024, 0);
		}
	}
}
