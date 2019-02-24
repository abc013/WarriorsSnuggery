/*
 * User: Andreas
 * Date: 01.10.2017
 * 
 */
using System;

namespace WarriorsSnuggery
{
	public struct WPos
	{
		public static readonly WPos Zero = new WPos();
		
		public readonly int X, Y, Z;
		
		public WPos(int x, int y, int z) { X = x; Y = y; Z = z; }
		
		public static WPos operator +(WPos lhs, WPos rhs) { return new WPos(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z); }
		
		public static WPos operator -(WPos lhs, WPos rhs) { return new WPos(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z); }
		
		public static bool operator ==(WPos lhs, WPos rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z; }

		public static bool operator !=(WPos lhs, WPos rhs) { return !(lhs == rhs); }
		
		public bool Equals(WPos pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is WPos && Equals((WPos) obj); }

		public override int GetHashCode() { return X ^ Y ^ Z; }
		
		public override string ToString() { return X + "," + Y + "," + Z; }
		
		public OpenTK.Vector4 ToVector()
		{
			return GLPos.ToVector(this);
		}
		
		public CPos ToCPos()
		{
			return new CPos(X * 1024, Y * 1024, Z * 1024);
		}
		
		public MPos ToMPos()
		{
			return new MPos(X,Y);
		}
	}
}
