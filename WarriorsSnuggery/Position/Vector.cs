using OpenTK;

namespace WarriorsSnuggery
{
	public static class GLPos
	{
		public const float PixelMultiplier = 1f / 24f;

		public static Vector4 FromPixel(int pxX, int pxY)
		{
			var x = (pxX / (float) WindowInfo.Width) * Camera.DefaultZoom * WindowInfo.Ratio;
			var y = (pxY / (float) WindowInfo.Height) * Camera.DefaultZoom;
			return new Vector4(x,-y,0,0);
		}

		public static Vector4 FromScreen(int pxX, int pxY)
		{
			pxX -= WindowInfo.Width / 2;
			pxY -= WindowInfo.Height / 2;
			var x = (pxX / (float) WindowInfo.Width) * WindowInfo.Ratio;
			var y = (pxY / (float) WindowInfo.Height);
			return new Vector4(x,-y,0,0);
		}

		public static CPos ToCPos(Vector4 pos)
		{
			return new CPos((int) (pos.X * 1024), (int) (-pos.Y * 1024), (int) (-pos.Z * 1024));
		}

		public static Vector4 ToVector(CPos pos)
		{
			return ToVector(pos.X / 1024f, pos.Y / 1024f, pos.Z / 1024f);
		}

		public static Vector4 ToVector(WPos pos)
		{
			return ToVector(pos.X, pos.Y, pos.Z);
		}

		public static Vector4 ToVector(MPos pos)
		{
			return ToVector(pos.X, pos.Y, 0f);
		}

		static Vector4 ToVector(float x, float y, float z)
		{
			return new Vector4(x, -y, -z, 1.0f);
		}
	}
}
