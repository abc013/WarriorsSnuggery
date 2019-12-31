using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class BatchRenderer
	{
		readonly List<Batch> batches = new List<Batch>();

		const int bufferSize = 6000;
		readonly Vertex[] buffer;
		int offset;

		public BatchRenderer()
		{
			buffer = new Vertex[bufferSize];
		}

		public void Add(Vertex[] data)
		{
			foreach(var vertex in data)
			{
				buffer[offset++] = vertex;

				if (offset == bufferSize)
					push();
			}
		}

		void push()
		{
			foreach (var batch in batches)
			{
				if (batch.CurrentSize + bufferSize >= Settings.BatchSize)
					continue;

				batch.SetData(buffer, batch.CurrentSize, bufferSize);
				Array.Clear(buffer, 0, bufferSize);
				offset = 0;
				return;
			}

			var @new = new Batch();
			@new.SetData(buffer, bufferSize);
			batches.Add(@new);
			Array.Clear(buffer, 0, bufferSize);
			offset = 0;
		}

		public void SetCurrent()
		{
			MasterRenderer.BatchRenderer = this;
		}

		public void Render()
		{
			push();
			foreach (var batch in batches)
			{
				//batch.Push();
				batch.Bind();
				lock (MasterRenderer.GLLock)
				{
					GL.UseProgram(MasterRenderer.TextureShader);

					var mat = Matrix4.Identity;
					GL.UniformMatrix4(MasterRenderer.GetLocation(MasterRenderer.TextureShader, "modelView"), false, ref mat);
					GL.Uniform4(MasterRenderer.GetLocation(MasterRenderer.TextureShader, "objectColor"), Color.White);
					Program.CheckGraphicsError("GraphicsObject_Uniform");
				}
				batch.Render();
				// Reset so we can overwrite the buffer;
				batch.CurrentSize = 0;
			}
		}

		public void Clear()
		{
			foreach (var batch in batches)
				batch.Clear();
		}

		public void Dispose()
		{
			foreach (var batch in batches)
				batch.Dispose();
		}
	}
}
