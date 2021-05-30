using OpenTK.Graphics.OpenGL;

namespace WarriorsSnuggery.Graphics
{
	public class Image : Renderable
	{
		readonly Texture texture;

		public Image(Vertex[] vertices, Texture texture) : base(Shaders.TextureShader, vertices.Length)
		{
			createBuffer(vertices);
			this.texture = texture;
		}

		public static void createBuffer(Vertex[] vertices)
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

		public override void Bind()
		{
			lock (MasterRenderer.GLLock)
			{
				UseProgram();
				GL.BindVertexArray(VertexArrayID);
				GL.BindTexture(TextureTarget.Texture2D, texture.SheetID);
				Program.CheckGraphicsError("Image_Bind");
			}
		}
	}
}
