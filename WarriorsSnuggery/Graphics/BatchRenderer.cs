using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public class BatchRenderer
	{
		readonly List<Batch> batches = new List<Batch>();

		readonly static int bufferSize = Settings.BatchSize;
		readonly Vertex[] buffer;
		int[] textureIDs;
		int offset;
		bool added;

		public BatchRenderer()
		{
			buffer = new Vertex[bufferSize];
		}

		public void SetTextures(Sheet[] sheets, int used)
		{
			textureIDs = new int[used + 1];
			for (int i = 0; i < used + 1; i++)
				textureIDs[i] = sheets[i].TextureID;

			if (textureIDs.Length > 4)
				Log.WriteDebug(string.Format("Warning: BatchRenderer got {0} sheets, maximum is {1}", sheets.Length, 4));
		}

		public void SetTextures(int[] IDs)
		{
			textureIDs = IDs;

			if (textureIDs.Length > 4)
				Log.WriteDebug(string.Format("Warning: BatchRenderer got {0} sheets, maximum is {1}", IDs.Length, 4));
		}

		public void Add(Vertex[] data)
		{
			added = true;
			if (data.Length + offset >= bufferSize)
				push();

			Array.Copy(data, 0, buffer, offset, data.Length);
			offset += data.Length;
		}

		void push()
		{
			if (offset != bufferSize)
				Array.Clear(buffer, offset, bufferSize - offset);

			foreach (var batch in batches)
			{
				if (batch.CurrentSize >= Settings.BatchSize)
					continue;

				batch.SetData(buffer, batch.CurrentSize, bufferSize);
				offset = 0;
				return;
			}

			var @new = new Batch();
			@new.SetData(buffer, bufferSize);
			batches.Add(@new);
			offset = 0;
		}

		public void SetCurrent()
		{
			MasterRenderer.BatchRenderer = this;
		}

		public void Render()
		{
			if (!added)
				return;

			push();

			lock (MasterRenderer.GLLock)
			{
				GL.UseProgram(MasterRenderer.TextureShader);

				var mat = Matrix4.Identity;
				GL.UniformMatrix4(MasterRenderer.GetLocation(MasterRenderer.TextureShader, "modelView"), false, ref mat);
				GL.Uniform4(MasterRenderer.GetLocation(MasterRenderer.TextureShader, "objectColor"), Color.White);
				Program.CheckGraphicsError("BatchRenderer_Uniform");
				for (int i = 0; i < textureIDs.Length; i++)
				{
					GL.ActiveTexture(TextureUnit.Texture0 + i);
					GL.BindTexture(TextureTarget.Texture2D, textureIDs[i]);
					Program.CheckGraphicsError("BatchRenderer_Texture" + i);
				}
				GL.ActiveTexture(TextureUnit.Texture0);
			}
			foreach (var batch in batches)
			{
				batch.Bind();
				batch.Render();
				// Reset so we can overwrite the buffer;
				batch.CurrentSize = 0;
			}

			added = false;
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
