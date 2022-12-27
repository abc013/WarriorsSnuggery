using OpenTK.Graphics.OpenGL;
using System;

namespace WarriorsSnuggery.Graphics
{
	public abstract class Renderable : IDisposable
	{
		public readonly Shader Shader;
		protected readonly int VertexArrayID;
		protected readonly int BufferID;
		protected readonly int VerticeCount;

		public Renderable(Shader shader, int vertexCount)
		{
			Shader = shader;
			VerticeCount = vertexCount;

			lock (MasterRenderer.GLLock)
			{
				VertexArrayID = GL.GenVertexArray();
				BufferID = GL.GenBuffer();
				Program.CheckGraphicsError("RenderableCreate_Generation");

				GL.BindVertexArray(VertexArrayID);
				GL.BindBuffer(BufferTarget.ArrayBuffer, BufferID);
				Program.CheckGraphicsError("RenderableCreate_Bind");
			}
		}

		public virtual void Bind()
		{
			lock (MasterRenderer.GLLock)
			{
				GL.UseProgram(Shader.ID);
				GL.BindVertexArray(VertexArrayID);
				Program.CheckGraphicsError("Renderable_Bind");
			}
		}

		public virtual void Render()
		{
			lock (MasterRenderer.GLLock)
			{
				GL.DrawArrays(PrimitiveType.Triangles, 0, VerticeCount);
				Program.CheckGraphicsError("Renderable_Draw");
			}
			MasterRenderer.RenderCalls++;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (MasterRenderer.GLLock)
				{
					GL.DeleteVertexArray(VertexArrayID);
					GL.DeleteBuffer(BufferID);
					Program.CheckGraphicsError("Renderable_Delete");
				}
			}
		}
	}
}
