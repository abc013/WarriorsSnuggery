using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;

namespace WarriorsSnuggery.Graphics
{
	public class ShaderProgram : IDisposable
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
