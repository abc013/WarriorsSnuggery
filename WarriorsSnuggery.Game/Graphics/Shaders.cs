using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace WarriorsSnuggery.Graphics
{
	public static class Shaders
	{
		public const int ShaderCount = 1;
		public const int UniformCount = 4;

		public static int TextureShader { get; private set; }

		static int programCount;
		static readonly ShaderProgram[] programs = new ShaderProgram[ShaderCount];

		static readonly int[] locations = new int[ShaderCount * UniformCount];

		public static void Initialize()
		{
			lock (MasterRenderer.GLLock)
			{
				TextureShader = createShader("Tex");

				foreach (int shader in new[] { TextureShader })
				{
					var num = UniformCount * (shader - 1);

					locations[num] = GL.GetUniformLocation(shader, "projection");
					locations[num + 1] = GL.GetUniformLocation(shader, "modelView");
					locations[num + 2] = GL.GetUniformLocation(shader, "proximityColor");
					locations[num + 3] = GL.GetUniformLocation(shader, "objectColor");

					GL.BindAttribLocation(shader, 0, "position");

					Log.Debug($"SHADER{shader} locations: {string.Join(',', locations)}");
				}

				GL.BindAttribLocation(TextureShader, 1, "textureCoordinate");
				GL.BindAttribLocation(TextureShader, 2, "color");

				foreach (int shader in new[] { TextureShader })
				{
					GL.UseProgram(shader);
					for (int sheetNum = 0; sheetNum < Settings.MaxSheets; sheetNum++)
					{
						var sheet = GL.GetUniformLocation(shader, "texture" + sheetNum);
						GL.Uniform1(sheet, sheetNum);
					}
				}

				Program.CheckGraphicsError("InitShaders");
			}
		}

		static int createShader(string name)
		{
			var program = new ShaderProgram();
			program.AddShader(ShaderType.VertexShader, FileExplorer.Shaders + name + ".vert");
			program.AddShader(ShaderType.FragmentShader, FileExplorer.Shaders + name + ".frag");
			program.Link();

			programs[programCount++] = program;

			return program.ID;
		}

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

		public static void Uniform(int shader, ref Matrix4 projection, Color ambient)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.UseProgram(shader);
				GL.UniformMatrix4(GetLocation(shader, "projection"), false, ref projection);
				GL.Uniform4(GetLocation(shader, "proximityColor"), ambient.ToColor4());
			}
		}

		public static void Dispose()
		{
			lock (MasterRenderer.GLLock)
			{
				foreach (var program in programs)
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
