/*
 * User: Andreas
 * Date: 11.08.2017
 * 
 */
using OpenTK.Graphics.ES30;
using System;

namespace WarriorsSnuggery.Graphics
{
	public abstract class Renderable : IDisposable
	{
		public readonly int ProgramID;
		protected readonly int VertexArrayID;
		protected readonly int BufferID;
		protected readonly int VerticeCount;

		public Renderable(int program, int vertexCount)
		{
			ProgramID = program;
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

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void Bind()
		{
			lock (MasterRenderer.GLLock)
			{
				UseProgram();
				GL.BindVertexArray(VertexArrayID);
				Program.CheckGraphicsError("Renderable_Bind");
			}
		}

		public void UseProgram()
		{
			if (MasterRenderer.RenderShadow)
				GL.UseProgram(MasterRenderer.ShadowShader);
			else
				GL.UseProgram(ProgramID);
			Program.CheckGraphicsError("Renderable_Program");
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
