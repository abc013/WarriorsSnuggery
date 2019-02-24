/*
 * User: Andreas
 * Date: 11.08.2017
 */
using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class IImage : Renderable
	{
		static readonly Dictionary<int, IImage> Images = new Dictionary<int, IImage>();

		public static IImage Create(TexturedVertex[] vertices, ITexture info)
		{
			IImage image;

			int key = info.GetHashCode();
			foreach(var vertex in vertices)
				key ^= vertex.GetHashCode();

			if (Images.ContainsKey(key))
			{
				image = Images[key];
			}
			else
			{
				image = new IImage(vertices, info);
				Images.Add(key, image);
			}

			return image;
		}

		public static void DisposeImages()
		{
			foreach (var sprite in Images.Values)
				sprite.Dispose();

			Images.Clear();
		}

		public static void CreateTextureBuffer(TexturedVertex[] vertices, int buffer, int vertexArray)
		{
			lock(MasterRenderer.GLLock)
			{
				GL.BufferData(BufferTarget.ArrayBuffer, TexturedVertex.Size * vertices.Length, vertices, BufferUsageHint.StaticDraw);

				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, true, TexturedVertex.Size, 0);

				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, true, TexturedVertex.Size, 16);
			}
		}

		public readonly ITexture Texture;

		IImage(TexturedVertex[] vertices, ITexture texture) : base(MasterRenderer.TextureShader, vertices.Length)
		{
			CreateTextureBuffer(vertices, BufferID, VertexArrayID);
			Texture = texture;
		}

		public override void Bind()
		{
			lock(MasterRenderer.GLLock)
			{
				UseProgram();
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindVertexArray(VertexArrayID);
				GL.BindTexture(TextureTarget.Texture2D, Texture.ID);
			}
		}
	}
}
