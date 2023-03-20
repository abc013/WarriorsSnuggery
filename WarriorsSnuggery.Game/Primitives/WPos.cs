using System;

namespace WarriorsSnuggery
{
	public readonly struct WPos
	{
		public static readonly WPos Zero = new WPos();

		public readonly int X;
		public readonly int Y;

		public float SquaredDist => X * (long)X + Y * (long)Y;
		public float Dist => (float)Math.Sqrt(SquaredDist);

		WPos(int x, int y)
		{
			X = x;
			Y = y;
		}

		public WPos (MPos terrainPos, bool horizontal) : this(terrainPos.X, terrainPos.Y, horizontal) { }

		public WPos(int terrainX, int terrainY, bool horizontal)
		{
			X = 2 * terrainX + (horizontal ? 1 : 0);
			Y = terrainY;
		}

		public static WPos operator +(in WPos lhs, in WPos rhs) { return new WPos(lhs.X + rhs.X, lhs.Y + rhs.Y); }
		public static WPos operator -(in WPos lhs, in WPos rhs) { return new WPos(lhs.X - rhs.X, lhs.Y - rhs.Y); }

		public static bool operator ==(in WPos lhs, in WPos rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y; }
		public static bool operator !=(in WPos lhs, in WPos rhs) { return !(lhs == rhs); }

		public bool Equals(WPos pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is WPos pos && Equals(pos); }

		public override int GetHashCode() { return X ^ Y; }

		public override string ToString() { return X + ", " + Y; }

		public MPos ToMPos()
		{
			return new MPos(X / 2, Y);
		}

		public bool IsHorizontal()
		{
			return X % 2 != 0;
		}

		public CPos ToCPos()
		{
			return new CPos(X * Constants.TileSize / 2, Y * Constants.TileSize, 0);
		}
	}
}
