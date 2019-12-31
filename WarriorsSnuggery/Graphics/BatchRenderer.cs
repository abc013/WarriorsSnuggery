using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class BatchRenderer
	{
		readonly List<Batch> batches = new List<Batch>();

		public BatchRenderer()
		{
		}

		public void Add(Vertex[] data)
		{
			foreach(var batch in batches)
			{
				if (batch.CurrentSize + data.Length >= Settings.BatchSize)
					continue;

				batch.SetData(data, batch.CurrentSize, data.Length);
				return;
			}

			var @new = new Batch();
			@new.SetData(data, data.Length);
			batches.Add(@new);
		}

		public void SetCurrent()
		{
			MasterRenderer.BatchRenderer = this;
		}

		public void Render()
		{
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
