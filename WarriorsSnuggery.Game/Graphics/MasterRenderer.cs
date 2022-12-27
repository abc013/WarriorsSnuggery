using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public enum Renderer
	{
		DEFAULT,
		LIGHTS,
		DEBUG
	}

	public static class MasterRenderer
	{
		public static int RenderCalls;
		public static int BatchCalls;
		public static int Batches;

		public static bool PauseSequences;
		public static object GLLock = new object();

		public static bool DrawAsLines;

		static Renderer activeRenderer = Renderer.DEFAULT;

		static readonly BatchRenderer defaultRenderer = new BatchRenderer();
		static readonly BatchRenderer lightRenderer = new BatchRenderer();
		static readonly BatchRenderer debugRenderer = new BatchRenderer();

		static FrameBuffer pixelFrameBuffer;

		public static void InitRenderer()
		{
			defaultRenderer.SetTextures(SheetManager.Sheets, SheetManager.SheetsUsed);
			lightRenderer.SetTextures(SheetManager.Sheets, SheetManager.SheetsUsed);
			debugRenderer.SetTextures(new[] { 0 });
		}

		public static void SetRenderer(Renderer renderer)
		{
			activeRenderer = renderer;
		}

		public static void AddToBatch(Vertex[] vertices)
		{
			switch (activeRenderer)
			{
				case Renderer.DEFAULT:
					defaultRenderer.Add(vertices);
					break;
				case Renderer.LIGHTS:
					lightRenderer.Add(vertices);
					break;
				case Renderer.DEBUG:
					debugRenderer.Add(vertices);
					break;
			}
		}

		public static void RenderBatch(bool asLines = false)
		{
			DrawAsLines = asLines;
			switch (activeRenderer)
			{
				case Renderer.DEFAULT:
					defaultRenderer.Render();
					break;
				case Renderer.LIGHTS:
					EnableLightBlending();
					lightRenderer.Render();
					DisableLightBlending();
					break;
				case Renderer.DEBUG:
					debugRenderer.Render();
					break;
			}
			DrawAsLines = false;
		}

		public static void Initialize()
		{
			var watch = Timer.Start();

			initializeGL();

			Shader.Initialize();
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

				var width = (int)(Camera.DefaultZoom * WindowInfo.Ratio * Constants.PixelSize);
				var height = (int)(Camera.DefaultZoom * Constants.PixelSize);

				pixelFrameBuffer = new FrameBuffer(width, height);

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
					pixelFrameBuffer.Use();

					WorldRenderer.Render();

					FrameBuffer.UseDefault();

					UpdateViewport();

					pixelFrameBuffer.Render();
					Program.CheckGraphicsError("GLRendering_World");
				}
				else
				{
					FrameBuffer.UseDefault();

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
				Program.CheckGraphicsError("GLLineWidth");
			}
		}


		public static void EnableLightBlending()
		{
			lock (GLLock)
			{
				GL.BlendFunc(BlendingFactor.DstColor, BlendingFactor.One);
				Program.CheckGraphicsError("GLLightBlending");
			}
		}

		public static void DisableLightBlending()
		{
			lock (GLLock)
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				Program.CheckGraphicsError("GLLightBlending");
			}
		}

		public static void UpdateViewport()
		{
			if (pixelFrameBuffer != null)
			{
				var width = (int)(Camera.DefaultZoom * WindowInfo.Ratio * Constants.PixelSize);
				var height = (int)(Camera.DefaultZoom * Constants.PixelSize);

				pixelFrameBuffer.Resize(width, height);
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
			defaultRenderer.Dispose();
			lightRenderer.Dispose();
			debugRenderer.Dispose();

			Shader.DisposeShaders();

			pixelFrameBuffer.Dispose();
		}
	}
}
