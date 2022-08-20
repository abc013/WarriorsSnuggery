using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public readonly struct UIPos
	{
		public static readonly UIPos Zero = new UIPos();

		readonly CPos intern;
		public int X => intern.X;
		public int Y => intern.Y;

		public UIPos(int x, int y)
		{
			intern = new CPos(x, y, 0);
		}

		public static UIPos operator +(in UIPos lhs, in UIPos rhs) { return new UIPos(lhs.X + rhs.X, lhs.Y + rhs.Y); }
		public static UIPos operator -(in UIPos lhs, in UIPos rhs) { return new UIPos(lhs.X - rhs.X, lhs.Y - rhs.Y); }
		public static UIPos operator *(in UIPos lhs, float rhs) { return new UIPos((int)(lhs.X * rhs), (int)(lhs.Y * rhs)); }
		public static UIPos operator *(float lhs, in UIPos rhs) { return rhs * lhs; }
		public static UIPos operator /(in UIPos lhs, float rhs) { return new UIPos((int)(lhs.X / rhs), (int)(lhs.Y / rhs)); }

		public static bool operator ==(in UIPos lhs, in UIPos rhs) { return lhs.X == rhs.X && lhs.Y == rhs.Y; }
		public static bool operator !=(in UIPos lhs, in UIPos rhs) { return !(lhs == rhs); }

		public static implicit operator CPos(in UIPos pos)
		{
			return pos.intern;
		}

		public bool Equals(in UIPos pos) { return pos == this; }
		public override bool Equals(object obj) { return obj is UIPos pos && Equals(pos); }

		public override int GetHashCode() { return X ^ Y; }

		public override string ToString() { return X + ", " + Y; }

		public static UIPos FromPixelPosition(float x, float y)
		{
			var rX = (x / WindowInfo.Width - 0.5f) * WindowInfo.Ratio;
			var rY = y / WindowInfo.Height - 0.5f;

			return FromRelativePosition(rX, rY);
		}

		public static UIPos FromRelativePosition(float x, float y)
		{
			var uX = (int)(x * Camera.UIZoom * Constants.TileSize);
			var uY = (int)(y * Camera.UIZoom * Constants.TileSize);

			return new UIPos(uX, uY);
		}

		public bool IsEmpty() => X == 0 && Y == 0;
	}
}
