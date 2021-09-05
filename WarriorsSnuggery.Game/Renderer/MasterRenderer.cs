using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public static class MasterRenderer
	{
		public const int PixelSize = 24;
		public const float PixelMultiplier = 1f / PixelSize;

		public static int RenderCalls;
		public static int BatchCalls;
		public static int Batches;

		public static bool PauseSequences;
		public static object GLLock = new object();

		public static PrimitiveType PrimitiveType = PrimitiveType.Triangles;

		public static bool UseDebugRenderer;

		static readonly BatchRenderer renderer = new BatchRenderer();
		static readonly BatchRenderer debugRenderer = new BatchRenderer();

		static int frameBuffer;
		static TexturedRenderable renderable;
		static Texture frameTexture;

		public static void InitRenderer()
		{
			renderer.SetTextures(SheetManager.Sheets, SheetManager.SheetsUsed);
			debugRenderer.SetTextures(new[] { 0 });

			WorldRenderer.Initialize();
			UIRenderer.Initialize();
		}

		public static void ResetRenderer(Game game)
		{
			PauseSequences = false;
			WorldRenderer.Reset(game);
			UIRenderer.Reset(game);
		}

		public static void AddToBatch(Vertex[] vertices)
		{
			if (UseDebugRenderer)
				debugRenderer.Add(vertices);
			else
				renderer.Add(vertices);
		}

		public static void RenderBatch()
		{
			if (UseDebugRenderer)
				debugRenderer.Render();
			else
				renderer.Render();
		}

		public static void Initialize()
		{
			var watch = Timer.Start();

			initializeGL();

			Shaders.Initialize();
			ColorManager.Initialize();

			watch.StopAndWrite("Configuring GL");
		}

		static void initializeGL()
		{
			lock (GLLock)
			{
				GL.ClearColor(Color4.Black);

				GL.LineWidth(ColorManager.DefaultLineWidth);
				Program.CheckGraphicsError("GLTests");

				GL.Enable(EnableCap.ScissorTest);
				GL.Enable(EnableCap.Blend);
				Program.CheckGraphicsError("GLTests");

				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				Program.CheckGraphicsError("GLEquations");

				var width = (int)(Camera.DefaultZoom * WindowInfo.Ratio * PixelSize);
				var height = (int)(Camera.DefaultZoom * PixelSize);

				var frameTextureID = GL.GenTexture();

				GL.BindTexture(TextureTarget.Texture2D, frameTextureID);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, width, height, 0, PixelFormat.Rgb, PixelType.Float, (IntPtr)null);

				frameBuffer = GL.GenFramebuffer();
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
				GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, frameTextureID, 0);
				frameTexture = new Texture(0, 0, width, height, frameTextureID);
				renderable = new TexturedRenderable(Mesh.Frame(), frameTexture);
				Program.CheckGraphicsError("GLFrameBuffer");

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				Program.CheckGraphicsError("GLTextures");

				GL.Scissor(0, 0, WindowInfo.Width, WindowInfo.Height);
				Program.CheckGraphicsError("GLScissor");
			}
		}

		public static void Render()
		{
			lock (GLLock)
			{
				RenderCalls = 0;
				BatchCalls = 0;
				Batches = 0;
				if (Settings.EnablePixeling)
				{
					GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
					GL.Clear(ClearBufferMask.ColorBufferBit);

					var width = (int)(Camera.DefaultZoom * WindowInfo.Ratio * PixelSize);
					var height = (int)(Camera.DefaultZoom * PixelSize);
					GL.Viewport(0, 0, width, height);
					GL.Scissor(0, 0, width, height);

					WorldRenderer.Render();

					GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
					GL.Clear(ClearBufferMask.ColorBufferBit);

					UpdateViewport();

					renderable.Bind();

					var iden = Matrix4.CreateScale(1f);
					Shaders.Uniform(Shaders.TextureShader, ref iden, Color.White);

					renderable.Render();
					Program.CheckGraphicsError("GLRendering_World");
				}
				else
				{
					GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
					GL.Clear(ClearBufferMask.ColorBufferBit);

					WorldRenderer.Render();
					Program.CheckGraphicsError("GLRendering_World");
				}

				UIRenderer.Render();
				Program.CheckGraphicsError("GLRendering_UI");
			}
		}

		public static void SetLineWidth(float width)
		{
			lock (GLLock)
			{
				GL.LineWidth(width);
				Program.CheckGraphicsError("LineWidth");
			}
		}

		public static void UpdateViewport()
		{
			lock (GLLock)
			{
				GL.Viewport(0, 0, WindowInfo.Width, WindowInfo.Height);
				Program.CheckGraphicsError("View_Viewport");
				GL.Scissor(0, 0, WindowInfo.Width, WindowInfo.Height);
				Program.CheckGraphicsError("View_Scissor");
			}

			Camera.Update();
			UIRenderer.Update();
		}

		public static void CreateScreenshot()
		{
			lock (GLLock)
			{
				var array = new byte[WindowInfo.Width * WindowInfo.Height * 4];
				GL.ReadPixels(0, 0, WindowInfo.Width, WindowInfo.Height, PixelFormat.Bgra, PixelType.UnsignedByte, array);

				FileExplorer.WriteScreenshot(array, WindowInfo.Width, WindowInfo.Height);
			}
		}

		public static void Dispose()
		{
			lock (GLLock)
			{
				GL.DeleteFramebuffer(frameBuffer);
			}

			renderer.Dispose();
			debugRenderer.Dispose();

			Shaders.Dispose();

			TextureManager.Dispose(frameTexture.SheetID);
			renderable.Dispose();
		}
	}
}
