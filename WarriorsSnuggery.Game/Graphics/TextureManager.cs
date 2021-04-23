using OpenTK.Graphics.OpenGL;
using System;

namespace WarriorsSnuggery.Graphics
{
	public static class TextureManager
	{
		public static int Create(int width, int height)
		{
			lock (MasterRenderer.GLLock)
			{
				int id = GL.GenTexture();

				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, id);

				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);

				Program.CheckGraphicsError("createTexture");

				return id;
			}
		}

		public static void Write(int id, float[] data, int offsetx, int offsety, int width, int height)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, id);

				GL.TexSubImage2D(TextureTarget.Texture2D, 0, offsetx, offsety, width, height, PixelFormat.Rgba, PixelType.Float, data);

				Program.CheckGraphicsError("writeTexture");
			}
		}

		public static float[] GetContent(int id, int width, int height)
		{
			var data = new float[width * height * 4];
			lock (MasterRenderer.GLLock)
			{
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, id);

				GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.Float, data);

				Program.CheckGraphicsError("readTexture");
			}

			return data;
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
