using System;
using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class Renderable : IDisposable
	{
		public readonly int ProgramID;
		protected readonly int VertexArrayID;
		protected readonly int BufferID;
		protected readonly int VerticeCount;

		public Renderable(int program, int vertexCount)
		{
			ProgramID = program;
			VerticeCount = vertexCount;

			lock(MasterRenderer.GLLock)
			{
				VertexArrayID = GL.GenVertexArray();
				BufferID = GL.GenBuffer();

				GL.BindVertexArray(VertexArrayID);
				GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void Bind()
		{
			lock(MasterRenderer.GLLock)
			{
				UseProgram();
				GL.BindVertexArray(VertexArrayID);
			}
		}

		public void UseProgram()
		{
			if (MasterRenderer.RenderShadow)
				GL.UseProgram(MasterRenderer.ShadowShader);
			else
				GL.UseProgram(ProgramID);
		}

		public virtual void Render()
		{
			lock(MasterRenderer.GLLock)
			{
				GL.DrawArrays(PrimitiveType.Triangles, 0, VerticeCount);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock(MasterRenderer.GLLock)
				{
					GL.DeleteVertexArray(VertexArrayID);
					GL.DeleteBuffer(BufferID);
				}
			}
		}
	}
}
