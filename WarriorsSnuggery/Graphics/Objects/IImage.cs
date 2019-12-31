using OpenTK.Graphics.ES30;
using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public class IImage : Renderable
	{
		static readonly Dictionary<int, IImage> images = new Dictionary<int, IImage>();

		public static IImage Create(Vertex[] vertices, ITexture info)
		{
			IImage image;

			int key = info.GetHashCode();
			foreach (var vertex in vertices)
				key ^= vertex.GetHashCode();

			if (images.ContainsKey(key))
			{
				image = images[key];
			}
			else
			{
				image = new IImage(vertices, info);
				images.Add(key, image);
			}

			return image;
		}

		public static void DisposeImages()
		{
			foreach (var image in images.Values)
				image.Dispose();

			images.Clear();
		}

		public static void CreateTextureBuffer(Vertex[] vertices)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.BufferData(BufferTarget.ArrayBuffer, Vertex.Size * vertices.Length, vertices, BufferUsageHint.StaticDraw);
				Program.CheckGraphicsError("CreateTexture_Buffer");

				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, true, Vertex.Size, 0);
				Program.CheckGraphicsError("CreateTexture_VertexArray1");

				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, true, Vertex.Size, 16);
				Program.CheckGraphicsError("CreateTexture_VertexArray2");

				GL.EnableVertexAttribArray(2);
				GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, true, Vertex.Size, 32);
				Program.CheckGraphicsError("CreateTexture_VertexArray3");
			}
		}

		public readonly ITexture Texture;

		IImage(Vertex[] vertices, ITexture texture) : base(MasterRenderer.TextureShader, vertices.Length)
		{
			CreateTextureBuffer(vertices);
			Texture = texture;
		}

		public override void Bind()
		{
			lock (MasterRenderer.GLLock)
			{
				UseProgram();
				GL.BindVertexArray(VertexArrayID);
				Program.CheckGraphicsError("ImageBind_Array");
				GL.BindTexture(TextureTarget.Texture2D, Texture.SheetID);
				Program.CheckGraphicsError("ImageBind_Texture");
			}
		}
	}
}
