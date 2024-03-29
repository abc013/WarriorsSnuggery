﻿using OpenTK.Graphics.OpenGL;
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
			var ids = new int[used];
			for (int i = 0; i < used; i++)
				ids[i] = sheets[i].TextureID;

			SetTextures(ids);
		}

		public void SetTextures(int[] IDs)
		{
			textureIDs = IDs;

			if (textureIDs.Length > Settings.MaxSheets)
				Log.Warning($"BatchRenderer has {IDs.Length} sheets, maximum is {Settings.MaxSheets}.");
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
			foreach (var batch in batches)
			{
				if (batch.CurrentSize + offset >= Settings.BatchSize)
					continue;

				batch.SetData(buffer, batch.CurrentSize, offset);
				offset = 0;
				return;
			}

			var @new = new Batch();
			@new.SetData(buffer, offset);
			batches.Add(@new);
			offset = 0;
		}

		public void Render()
		{
			if (!added)
				return;

			push();

			lock (MasterRenderer.GLLock)
			{
				GL.UseProgram(Shader.TextureShader.ID);

				Program.CheckGraphicsError("BatchRenderer_Uniform");
				for (int i = 0; i < textureIDs.Length; i++)
				{
					GL.ActiveTexture(TextureUnit.Texture0 + i);
					GL.BindTexture(TextureTarget.Texture2D, textureIDs[i]);
					Program.CheckGraphicsError("BatchRenderer_Texture" + i);
				}
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

		public void Dispose()
		{
			foreach (var batch in batches)
				batch.Dispose();
		}
	}
}
