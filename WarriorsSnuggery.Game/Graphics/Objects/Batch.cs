﻿using OpenTK.Graphics.OpenGL;
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

				TextureBuffer.ConfigureBugger(vertexarrayID, bufferID, Size, IntPtr.Zero, BufferUsageHint.StreamDraw);
			}
		}

		public void SetData(Vertex[] data, int length)
		{
			SetData(data, 0, length);
		}

		public void SetData(Vertex[] data, int start, int length)
		{
			if (start + length > Settings.BatchSize)
			{
				Log.Warning($"Unable to push vertices to batch: target ({start * Vertex.Size}, {length * Vertex.Size}) exceeds size of buffer ({Size}).");
				return;
			}

			Bind();
			lock (MasterRenderer.GLLock)
			{
				GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(start * Vertex.Size), length * Vertex.Size, data);
				Program.CheckGraphicsError("BatchData");
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
				GL.DrawArrays(MasterRenderer.DrawAsLines ? PrimitiveType.Lines : PrimitiveType.Triangles, 0, CurrentSize);
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
