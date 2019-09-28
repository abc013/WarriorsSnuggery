using OpenTK.Graphics.ES30;
using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public class IImage : Renderable
	{
		static readonly Dictionary<int, IImage> Images = new Dictionary<int, IImage>();

		public static IImage Create(TexturedVertex[] vertices, ITexture info)
		{
			IImage image;

			int key = info.GetHashCode();
			foreach (var vertex in vertices)
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
			foreach (var image in Images.Values)
				image.Dispose();

			Images.Clear();
		}

		public static void CreateTextureBuffer(TexturedVertex[] vertices)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.BufferData(BufferTarget.ArrayBuffer, TexturedVertex.Size * vertices.Length, vertices, BufferUsageHint.StaticDraw);
				Program.CheckGraphicsError("CreateTexture_Buffer");

				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, true, TexturedVertex.Size, 0);
				Program.CheckGraphicsError("CreateTexture_VertexArray1");

				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, true, TexturedVertex.Size, 16);
				Program.CheckGraphicsError("CreateTexture_VertexArray2");
			}
		}

		public readonly ITexture Texture;

		IImage(TexturedVertex[] vertices, ITexture texture) : base(MasterRenderer.TextureShader, vertices.Length)
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
