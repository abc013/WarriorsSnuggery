using OpenTK.Graphics.OpenGL;

namespace WarriorsSnuggery.Graphics
{
	public static class TextureManager
	{
		public static int TextureCount;

		public static int Create(MPos size)
		{
			lock (MasterRenderer.GLLock)
			{
				int id = GL.GenTexture();
				TextureCount++;

				var emptyData = new float[size.X * size.Y * 4];
				emptyData.Initialize();

				Write(id, emptyData, size);

				Program.CheckGraphicsError("createTexture");

				return id;
			}
		}

		public static void Write(int id, float[] data, MPos size)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, id);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, size.X, size.Y, 0, PixelFormat.Rgba, PixelType.Float, data);

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);

				Program.CheckGraphicsError("writeTexture");
			}
		}

		public static void Dispose(int id)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.DeleteTexture(id);

				Program.CheckGraphicsError("disposeTexture");
			}
		}
	}
}
