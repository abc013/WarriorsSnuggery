using System;
using OpenTK;
using OpenTK.Graphics.ES30;
using OpenTK.Graphics;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public static class MasterRenderer
	{
		public static object GLLock = new object();
		public static int ColorShader, TextureShader, FontShader, ShadowShader;
		static int heightLocation;
		static readonly int[] locations = new int[16];

		public static int GetLocation(int shader, string name)
		{
			var shadernum = 4 * (shader - 1);
			int num = 0;
			switch(name)
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

		static readonly ShaderProgram[] shaders = new ShaderProgram[4];

		public static void ResetRenderer(Game game)
		{
			WorldRenderer.Reset(game);
			UIRenderer.Reset(game);
		}

		public static void Initialize()
		{
			var watch = new StopWatch();
			watch.Start();

			initializeShaders();
			initializeGL();
			ColorManager.Initialize();

			watch.Stop();
			Log.WritePerformance(watch.ElapsedMilliseconds, "Configuring GL");
		}

		static void initializeShaders()
		{
			lock(GLLock)
			{
				ColorShader = createShader("Col");
				TextureShader = createShader("Tex");
				FontShader = createShader("Fon");
				ShadowShader = createShader("Sha");

				foreach (int shader in new [] { ColorShader, TextureShader, FontShader, ShadowShader })
				{
					var num = 4 * (shader - 1);
					locations[num] = GL.GetUniformLocation(shader, "projection");
					locations[num + 1] = GL.GetUniformLocation(shader, "modelView");
					locations[num + 2] = GL.GetUniformLocation(shader, "proximityColor");
					locations[num + 3] = GL.GetUniformLocation(shader, "objectColor");

					GL.BindAttribLocation(shader, 0, "position");

					Log.WriteDebug("SHADER " + shader + " locations: " + locations[num] + ", " + locations[num + 1] + ", " + locations[num + 2] + ", " + locations[num + 3] + ";");
				}

				heightLocation = GL.GetUniformLocation(ShadowShader, "height");

				GL.BindAttribLocation(ColorShader, 1, "color"); 
				GL.BindAttribLocation(TextureShader, 1, "textureCoordinate");
				GL.BindAttribLocation(FontShader, 3, "textureOffset");
				GL.BindAttribLocation(ShadowShader, 1, "textureCoordinate");

				Program.CheckGraphicsError("InitShaders");
			}
		}

		static int programCount;
		static int createShader(string name)
		{
			var program = new ShaderProgram();
			program.AddShader(ShaderType.VertexShader, FileExplorer.Shaders +   @"\" + name + ".vert");
			program.AddShader(ShaderType.FragmentShader, FileExplorer.Shaders + @"\" + name + ".frag");
			program.Link();

			shaders[programCount] = program;
			programCount++;
			return program.ID;
		}

		static int frameBuffer;
		public static int frameTexture;
		static Objects.FrameRenderable renderable;

		static void initializeGL()
		{
			lock(GLLock)
			{
				GL.ClearColor(Color4.Black);

				GL.LineWidth(ColorManager.DefaultLineWidth);

				if (Settings.AntiAliasing)
				{
					EnableAliasing();
					//GL.Enable(EnableCap.PolygonSmooth); // WORKS, but you can see line in polygons
					//GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
				}

				GL.Enable(EnableCap.Texture2D);
				GL.Enable(EnableCap.ScissorTest);
				GL.Enable(EnableCap.AlphaTest);
				GL.Enable(EnableCap.Blend);
				//GL.Enable(EnableCap.CullFace); // GRAPHICS sth turns around (180°) and gets invisible
				GL.CullFace(CullFaceMode.Back);
				Program.CheckGraphicsError("GLTests");

				GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
				Program.CheckGraphicsError("GLEquations");

				var width = (int)(WindowInfo.UnitWidth * 24);
				var height = (int)(WindowInfo.UnitHeight * 24);

				frameTexture = GL.GenTexture();
				GL.BindTexture(TextureTarget.Texture2D, frameTexture);
				GL.TexImage2D(TextureTarget2d.Texture2D,0,TextureComponentCount.Rgb32f, width, height, 0, PixelFormat.Rgb, PixelType.Float, (IntPtr) null);

				frameBuffer = GL.GenFramebuffer();
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
				GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget2d.Texture2D, frameTexture, 0);
				renderable = new Objects.FrameRenderable(new ITexture("FramebufferTexture", width, height, frameTexture));
				Program.CheckGraphicsError("GLFrameBuffer");

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
				Program.CheckGraphicsError("GLTextures");

				GL.Scissor(0, 0, WindowInfo.Width, WindowInfo.Height);
				Program.CheckGraphicsError("GLScissor");
			}
		}

		public static void Render()
		{
			lock(GLLock)
			{
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			}

			if (Window.Loaded)
			{
				if (Settings.EnablePixeling)
				{
					GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

					var width = (int)(WindowInfo.UnitWidth * 24);
					var height = (int)(WindowInfo.UnitHeight * 24);
					GL.Viewport(0, 0, width, height);
					GL.Scissor(0, 0, width, height);

					WorldRenderer.Render();

					GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

					UpdateView();

					Matrix4 iden = Matrix4.CreateScale(1f, 1f, 1f);
					Uniform(TextureShader, ref iden, Color.White);

					renderable.Render();
					Program.CheckGraphicsError("GLRendering_World");

					UIRenderer.Render();
					Program.CheckGraphicsError("GLRendering_UI");
				}
				else
				{
					GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
					WorldRenderer.Render();
					Program.CheckGraphicsError("GLRendering_World");
					UIRenderer.Render();
					Program.CheckGraphicsError("GLRendering_UI");
				}
			}
		}

		public static void EnableAliasing()
		{
			lock (GLLock)
			{
				GL.Enable(EnableCap.LineSmooth);
				GL.Enable(EnableCap.PointSmooth);
			}
			Program.CheckGraphicsError("Aliasing_On");
		}

		public static void DisableAliasing()
		{
			lock (GLLock)
			{
				GL.Disable(EnableCap.LineSmooth);
				GL.Disable(EnableCap.PointSmooth);
			}
			Program.CheckGraphicsError("Aliasing_Off");
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

			Camera.Zoom(0);
			UIRenderer.Update();
		}

		public static void Dispose()
		{
			lock(GLLock)
			{
				foreach (var shader in shaders)
					shader.Dispose();
			}
			GL.DeleteFramebuffer(frameBuffer);
			GL.DeleteTexture(frameTexture);
		}

		public static bool RenderShadow;

		public static void UniformHeight(int height)
		{
			lock (GLLock)
			{
				GL.UseProgram(ShadowShader);
				Program.CheckGraphicsError("UniformHeight_Program");
				GL.Uniform1(heightLocation, height / 150f);
				Program.CheckGraphicsError("UniformHeight_Uniform");
				GL.UseProgram(0);
				Program.CheckGraphicsError("UniformHeight_Reset");
			}
		}

		public static void Uniform(int shader, ref Matrix4 projection, Color ambient)
		{
			lock(GLLock)
			{
				GL.UseProgram(shader);
				GL.UniformMatrix4(GetLocation(shader, "projection"), false, ref projection);
				GL.Uniform4(GetLocation(shader, "proximityColor"), ambient.toColor4());
			}
		}
	}

	public static class GLCommands
	{

	}
}
