using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public static class MasterRenderer
	{
		public static int RenderCalls;
		public static int BatchCalls;
		public static int Batches;

		public static BatchRenderer BatchRenderer;

		public const int PixelSize = 24;
		public const float PixelMultiplier = 1f / PixelSize;

		public static bool PauseSequences;
		public static object GLLock = new object();

		public static int TextureShader;
		static readonly int[] locations = new int[4];

		static readonly ShaderProgram[] shaders = new ShaderProgram[1];

		public static PrimitiveType PrimitiveType = PrimitiveType.Triangles;

		public static int GetLocation(int shader, string name)
		{
			var shadernum = 4 * (shader - 1);
			int num = 0;
			switch (name)
			{
				case "modelView":
					num = 1;
					break;
				case "proximityColor":
					num = 2;
					break;
				case "objectColor":
					num = 3;
					break;
			}
			return locations[num + shadernum];
		}

		public static void ResetRenderer(Game game)
		{
			PauseSequences = false;
			WorldRenderer.Reset(game);
			UIRenderer.Reset(game);
		}

		public static void Initialize()
		{
			var watch = Timer.Start();

			initializeShaders();
			initializeGL();
			ColorManager.Initialize();

			watch.StopAndWrite("Configuring GL");
		}

		static void initializeShaders()
		{
			lock (GLLock)
			{
				TextureShader = createShader("Tex");

				foreach (int shader in new[] { TextureShader })
				{
					var num = 4 * (shader - 1);
					locations[num] = GL.GetUniformLocation(shader, "projection");
					locations[num + 1] = GL.GetUniformLocation(shader, "modelView");
					locations[num + 2] = GL.GetUniformLocation(shader, "proximityColor");
					locations[num + 3] = GL.GetUniformLocation(shader, "objectColor");

					GL.BindAttribLocation(shader, 0, "position");

					Log.WriteDebug("SHADER " + shader + " locations: " + locations[num] + ", " + locations[num + 1] + ", " + locations[num + 2] + ", " + locations[num + 3] + ";");
				}

				GL.BindAttribLocation(TextureShader, 1, "textureCoordinate");
				GL.BindAttribLocation(TextureShader, 2, "color");

				foreach (int shader in new[] { TextureShader })
				{
					GL.UseProgram(shader);
					var tex1 = GL.GetUniformLocation(shader, "texture0");
					GL.Uniform1(tex1, 0);
					var tex2 = GL.GetUniformLocation(shader, "texture1");
					GL.Uniform1(tex2, 1);
					var tex3 = GL.GetUniformLocation(shader, "texture2");
					GL.Uniform1(tex3, 2);
					var tex4 = GL.GetUniformLocation(shader, "texture3");
					GL.Uniform1(tex4, 3);
				}

				Program.CheckGraphicsError("InitShaders");
			}
		}

		static int programCount;
		static int createShader(string name)
		{
			var program = new ShaderProgram();
			program.AddShader(ShaderType.VertexShader, FileExplorer.Shaders + name + ".vert");
			program.AddShader(ShaderType.FragmentShader, FileExplorer.Shaders + name + ".frag");
			program.Link();

			shaders[programCount] = program;
			programCount++;
			return program.ID;
		}

		static int frameBuffer;
		static Image renderable;
		static Texture frameTexture;

		static void initializeGL()
		{
			lock (GLLock)
			{
				GL.ClearColor(Color4.Black);

				GL.LineWidth(ColorManager.DefaultLineWidth);
				Program.CheckGraphicsError("GLTests");

				GL.Enable(EnableCap.ScissorTest);
				GL.Enable(EnableCap.Blend);
				//GL.Enable(EnableCap.AlphaTest); WUT why does this work
				Program.CheckGraphicsError("GLTests");

				GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				Program.CheckGraphicsError("GLEquations");

				var width = (int)(WindowInfo.UnitWidth * 24);
				var height = (int)(WindowInfo.UnitHeight * 24);

				var frameTextureID = GL.GenTexture();

				GL.BindTexture(TextureTarget.Texture2D, frameTextureID);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, width, height, 0, PixelFormat.Rgb, PixelType.Float, (IntPtr)null);

				frameBuffer = GL.GenFramebuffer();
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
				GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, frameTextureID, 0);
				frameTexture = new Texture(0, 0, width, height, frameTextureID);
				renderable = new Image(Mesh.Frame(), frameTexture);
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

					var width = (int)(WindowInfo.UnitWidth * 24);
					var height = (int)(WindowInfo.UnitHeight * 24);
					GL.Viewport(0, 0, width, height);
					GL.Scissor(0, 0, width, height);

					WorldRenderer.Render();

					GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

					UpdateView();

					Matrix4 iden = Matrix4.CreateScale(1f, 1f, 1f);
					Uniform(TextureShader, ref iden, Color.White);

					renderable.Bind();
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

		public static void UpdateView()
		{
			lock (GLLock)
			{
				GL.Viewport(0, 0, WindowInfo.Width, WindowInfo.Height);
				Program.CheckGraphicsError("View_Viewport");
				GL.Scissor(0, 0, WindowInfo.Width, WindowInfo.Height);
				Program.CheckGraphicsError("View_Scissor");
			}

			Camera.Reset(false);
			UIRenderer.Update();
		}

		public static void CreateScreenshot()
		{
			lock (GLLock)
			{
				var array = new byte[WindowInfo.Width * WindowInfo.Height * 3];
				GL.ReadPixels(0, 0, WindowInfo.Width, WindowInfo.Height, PixelFormat.Bgr, PixelType.UnsignedByte, array);

				FileExplorer.WriteScreenshot(array, WindowInfo.Width, WindowInfo.Height);
			}
		}

		public static void Uniform(int shader, ref Matrix4 projection, Color ambient)
		{
			lock (GLLock)
			{
				GL.UseProgram(shader);
				GL.UniformMatrix4(GetLocation(shader, "projection"), false, ref projection);
				GL.Uniform4(GetLocation(shader, "proximityColor"), ambient.ToColor4());
			}
		}

		public static void Dispose()
		{
			lock (GLLock)
			{
				foreach (var shader in shaders)
					shader.Dispose();
			}
			GL.DeleteFramebuffer(frameBuffer);
			TextureManager.Dispose(frameTexture.SheetID);
			renderable.Dispose();
		}
	}
}
