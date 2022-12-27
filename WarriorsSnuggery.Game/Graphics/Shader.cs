using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace WarriorsSnuggery.Graphics
{
	public class Shader
	{
		public const int ShaderCount = 1;
		public const int UniformCount = 5;

		public static Shader TextureShader { get; private set; }

		readonly int[] locations = new int[UniformCount];
		public readonly int ID;
		readonly ShaderProgram program;

		public static void Initialize()
		{
			TextureShader = new Shader("Tex");
		}

		public static void DisposeShaders()
		{
			TextureShader.Dispose();
			TextureShader = null;
		}

		public Shader(string name)
		{
			lock (MasterRenderer.GLLock)
			{
				program = new ShaderProgram();
				program.AddShader(ShaderType.VertexShader, FileExplorer.Shaders + name + ".vert");
				program.AddShader(ShaderType.FragmentShader, FileExplorer.Shaders + name + ".frag");
				program.Link();

				ID = program.ID;
			}

			configureShader();
		}

		void configureShader()
		{
			lock (MasterRenderer.GLLock)
			{
				locations[0] = GL.GetUniformLocation(ID, "projection");
				locations[1] = GL.GetUniformLocation(ID, "modelView");
				locations[2] = GL.GetUniformLocation(ID, "proximityColor");
				locations[3] = GL.GetUniformLocation(ID, "objectColor");
				locations[4] = GL.GetUniformLocation(ID, "hidePosition");

				GL.BindAttribLocation(ID, Vertex.PositionAttributeLocation, "position");

				Log.Debug($"Shader '{ID}' locations: {string.Join(',', locations)}");

				GL.BindAttribLocation(ID, Vertex.TextureCoordinateAttributeLocation, "textureCoordinate");
				GL.BindAttribLocation(ID, Vertex.TextureAttributeLocation, "texture");
				GL.BindAttribLocation(ID, Vertex.TextureFlagsAttributeLocation, "textureFlags");
				GL.BindAttribLocation(ID, Vertex.ColorAttributeLocation, "color");

				for (int sheetNum = 0; sheetNum < Settings.MaxSheets; sheetNum++)
				{
					var sheet = GL.GetUniformLocation(ID, "texture" + sheetNum);
					GL.Uniform1(sheet, sheetNum);
				}

				Program.CheckGraphicsError($"InitShader{ID}");
			}
		}

		public int GetLocation(string name)
		{
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
				case "hidePosition":
					num = 4;
					break;
			}
			return locations[num];
		}

		public void Uniform(ref Matrix4 projection, Color ambient, CPos hideOrigin)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.UseProgram(ID);
				GL.UniformMatrix4(GetLocation("projection"), false, ref projection);
				GL.Uniform4(GetLocation("proximityColor"), ambient.ToColor4());
				GL.Uniform3(GetLocation("hidePosition"), hideOrigin.ToVector());
			}
		}

		public void Dispose()
		{
			lock (MasterRenderer.GLLock)
			{
				program.Dispose();
			}
		}

		class ShaderProgram : IDisposable
		{
			public int ID;
			readonly List<int> shaders = new List<int>();

			public ShaderProgram()
			{
				lock (MasterRenderer.GLLock)
				{
					ID = GL.CreateProgram();
				}
			}

			public void AddShader(ShaderType type, string path)
			{
				lock (MasterRenderer.GLLock)
				{
					var shader = GL.CreateShader(type);
					GL.ShaderSource(shader, File.ReadAllText(path));
					GL.CompileShader(shader);

					var info = GL.GetShaderInfoLog(shader);
					if (!string.IsNullOrWhiteSpace(info))
						Log.Debug($"SHADER{shader} information: {info}");

					shaders.Add(shader);
				}
			}

			public void Link()
			{
				lock (MasterRenderer.GLLock)
				{
					foreach (var shader in shaders)
						GL.AttachShader(ID, shader);

					GL.LinkProgram(ID);

					var info = GL.GetProgramInfoLog(ID);
					if (!string.IsNullOrWhiteSpace(info))
						Log.Debug($"SHADER{ID} information: {info}");

					foreach (var shader in shaders)
					{
						GL.DetachShader(ID, shader);
						GL.DeleteShader(shader);
					}
				}
			}

			public virtual void Dispose()
			{
				dispose(true);
				GC.SuppressFinalize(this);
			}

			void dispose(bool disposing)
			{
				if (disposing)
				{
					lock (MasterRenderer.GLLock)
					{
						GL.DeleteProgram(ID);
					}
				}
			}
		}
	}
}
