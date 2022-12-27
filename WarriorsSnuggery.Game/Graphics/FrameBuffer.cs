using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;

namespace WarriorsSnuggery.Graphics
{
	public class FrameBuffer : IDisposable
	{
		readonly int frameBuffer;
		readonly int frameTextureID;

		Renderable renderable;

		int width;
		int height;

		public FrameBuffer(int width, int height)
		{
			lock (MasterRenderer.GLLock)
			{
				frameTextureID = GL.GenTexture();

				Resize(width, height);

				frameBuffer = GL.GenFramebuffer();
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
				GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, frameTextureID, 0);

				Program.CheckGraphicsError("GLFrameBuffer");
			}
		}

		public void Resize(int width, int height)
		{
			this.width = width;
			this.height = height;

			lock (MasterRenderer.GLLock)
			{
				GL.BindTexture(TextureTarget.Texture2D, frameTextureID);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, width, height, 0, PixelFormat.Rgb, PixelType.Float, (IntPtr)null);
			}

			renderable = new TexturedRenderable(Mesh.Frame(), new Texture(0, 0, width, height, frameTextureID));
		}

		public void Use()
		{
			useBuffer(frameBuffer);

			lock (MasterRenderer.GLLock)
			{
				GL.Viewport(0, 0, width, height);
				GL.Scissor(0, 0, width, height);
				Program.CheckGraphicsError("Viewport_Framebuffer");
			}
		}

		public void Render()
		{
			renderable.Bind();

			var iden = Matrix4.Identity;
			Shader.TextureShader.Uniform(ref iden, Color.White, CPos.Zero);

			renderable.Render();
		}

		public void Dispose()
		{
			lock (MasterRenderer.GLLock)
			{
				GL.DeleteFramebuffer(frameBuffer);
			}

			TextureManager.Dispose(frameTextureID);
			renderable.Dispose();
		}

		public static void UseDefault()
		{
			useBuffer(0);

			lock (MasterRenderer.GLLock)
			{
				GL.Viewport(0, 0, WindowInfo.Width, WindowInfo.Height);
				GL.Scissor(0, 0, WindowInfo.Width, WindowInfo.Height);
				Program.CheckGraphicsError("Viewport_Default");
			}
		}

		static void useBuffer(int frameBuffer)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
				GL.Clear(ClearBufferMask.ColorBufferBit);
			}
		}
	}
}
