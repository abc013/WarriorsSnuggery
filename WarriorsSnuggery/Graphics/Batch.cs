using OpenTK.Graphics.OpenGL;
using System;

namespace WarriorsSnuggery.Graphics
{
	public class Batch : IDisposable
	{
		public readonly static int Size = Settings.BatchSize * Vertex.Size;
		public int CurrentSize;

		readonly int vertexarrayID;
		readonly int bufferID;
		bool disposed;

		public Batch()
		{
			lock (MasterRenderer.GLLock)
			{
				vertexarrayID = GL.GenVertexArray();
				bufferID = GL.GenBuffer();
				Program.CheckGraphicsError("BatchInit_1");

				GL.BindVertexArray(vertexarrayID);
				GL.BindBuffer(BufferTarget.ArrayBuffer, bufferID);
				Program.CheckGraphicsError("BatchInit_2");

				GL.BufferData(BufferTarget.ArrayBuffer, Size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
				Program.CheckGraphicsError("BatchInit_3");

				// position
				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, true, Vertex.Size, 0);
				Program.CheckGraphicsError("BatchInit_4");

				// texture coordinates
				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, true, Vertex.Size, 16);
				Program.CheckGraphicsError("BatchInit_5");

				// color
				GL.EnableVertexAttribArray(2);
				GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, true, Vertex.Size, 32);
				Program.CheckGraphicsError("BatchInit_6");
			}
		}

		public void SetData(Vertex[] data, int length)
		{
			if (length > Settings.BatchSize)
			{
				Log.WriteDebug(string.Format("Unable to push vertices to batch: target ({0}) exceeds size of buffer ({1}).", length * Vertex.Size, Size));
				return;
			}

			Bind();
			lock (MasterRenderer.GLLock)
			{
				GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(0), length * Vertex.Size, data);
				Program.CheckGraphicsError("BatchData_1");
			}

			CurrentSize = length;

			MasterRenderer.BatchCalls++;
		}

		public void SetData(Vertex[] data, int start, int length)
		{
			if (start + length > Settings.BatchSize)
			{
				Log.WriteDebug(string.Format("Unable to push vertices to batch: target ({0}, {1}) exceeds size of buffer ({2}).", start * Vertex.Size, length * Vertex.Size, Size));
				return;
			}

			Bind();
			lock (MasterRenderer.GLLock)
			{
				GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(start * Vertex.Size), length * Vertex.Size, data);
				Program.CheckGraphicsError("BatchData_2");
			}

			if (CurrentSize < start + length)
				CurrentSize = start + length;

			MasterRenderer.BatchCalls++;
		}

		public void Bind()
		{
			lock (MasterRenderer.GLLock)
			{
				GL.BindVertexArray(vertexarrayID);
				GL.BindBuffer(BufferTarget.ArrayBuffer, bufferID);
				Program.CheckGraphicsError("BatchBind");
			}
		}

		public void Render()
		{
			if (CurrentSize == 0)
				return;

			lock (MasterRenderer.GLLock)
			{
				GL.DrawArrays(MasterRenderer.PrimitiveType, 0, CurrentSize);
			}
			MasterRenderer.Batches++;
		}

		public void Dispose()
		{
			if (disposed)
				return;

			disposed = true;

			lock (MasterRenderer.GLLock)
			{
				GL.DeleteVertexArray(vertexarrayID);
				GL.DeleteBuffer(bufferID);
			}
			GC.SuppressFinalize(this);
		}
	}
}
