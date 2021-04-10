using OpenTK.Mathematics;


namespace WarriorsSnuggery
{
	public readonly struct Color
	{
		public static readonly Color White = new Color(1f, 1f, 1f);
		public static readonly Color Grey = new Color(0.75f, 0.75f, 0.75f);
		public static readonly Color Black = new Color(0f, 0f, 0f);

		public static readonly Color Blue = new Color(0f, 0f, 1f);
		public static readonly Color Red = new Color(1f, 0f, 0f);
		public static readonly Color Green = new Color(0f, 1f, 0f);

		public static readonly Color Magenta = new Color(1f, 0f, 1f);
		public static readonly Color Yellow = new Color(1f, 1f, 0f);
		public static readonly Color Cyan = new Color(0f, 1f, 1f);

		public static readonly Color Shadow = new Color(0, 0, 0, 64);

		public readonly float R;

		public readonly float G;

		public readonly float B;

		public readonly float A;

		public static Color operator +(Color lhs, Color rhs) { return new Color(lhs.R + rhs.R, lhs.G + rhs.G, lhs.B + rhs.B, lhs.A + rhs.A); }

		public static Color operator -(Color lhs, Color rhs) { return new Color(lhs.R - rhs.R, lhs.G - rhs.G, lhs.B - rhs.B, lhs.A - rhs.A); }

		public static Color operator *(Color lhs, Color rhs) { return new Color(lhs.R * rhs.R, lhs.G * rhs.G, lhs.B * rhs.B, lhs.A * rhs.A); }

		public static Color operator /(Color lhs, Color rhs) { return new Color(lhs.R / rhs.R, lhs.G / rhs.G, lhs.B / rhs.B, lhs.A / rhs.A); }

		public static implicit operator Color(System.Drawing.Color color)
		{
			return new Color(color.R, color.G, color.B, color.A);
		}

		public static implicit operator System.Drawing.Color(Color color)
		{
			return System.Drawing.Color.FromArgb((int)(color.A * 255f), (int)(color.R * 255f), (int)(color.G * 255f), (int)(color.B * 255f));
		}

		public static implicit operator Color(Color4 color)
		{
			return new Color(color.R, color.G, color.B, color.A);
		}

		public static implicit operator Color4(Color color)
		{
			return new Color4(color.R, color.G, color.B, color.A);
		}

		public static bool operator !=(Color lhf, Color rhf)
		{
			return !(lhf == rhf);
		}

		public static bool operator ==(Color lhf, Color rhf)
		{
			return lhf.R == rhf.R && lhf.G == rhf.G && lhf.B == rhf.B && lhf.A == rhf.A;
		}

		public Color4 ToColor4()
		{
			return new Color4(R, G, B, A);
		}

		public Vector4 ToVector4()
		{
			return new Vector4(R, G, B, A);
		}

		public System.Drawing.Color ToSysColor()
		{
			return System.Drawing.Color.FromArgb((int)(A * 255f), (int)(R * 255f), (int)(G * 255f), (int)(B * 255f));
		}

		public override string ToString()
		{
			return $"COLOR({R} | {G} | {B} | {A})";
		}

		public override bool Equals(object obj)
		{
			return obj is Color color && color == this;
		}

		public override int GetHashCode()
		{
			return (int)(R * 255) ^ (int)(G * 255) ^ (int)(B * 255) ^ (int)(A * 255);
		}

		public Color(int r, int g, int b, int a = 255)
		{
			R = r / 255f;
			G = g / 255f;
			B = b / 255f;
			A = a / 255f;
		}

		public Color(float r, float g, float b, float a = 1)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}
	}
}
