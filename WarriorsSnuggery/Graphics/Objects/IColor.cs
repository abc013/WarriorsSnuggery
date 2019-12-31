using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class IColor : Renderable
	{
		public IColor(Vertex[] vertices) : base(MasterRenderer.TextureShader, vertices.Length)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.BufferData(BufferTarget.ArrayBuffer, Vertex.Size * vertices.Length, vertices, BufferUsageHint.StaticDraw);
				Program.CheckGraphicsError("ColorCreate_Buffer");

				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, true, Vertex.Size, 0);
				Program.CheckGraphicsError("ColorCreate_VertexArray1");

				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, true, Vertex.Size, 16);
				Program.CheckGraphicsError("ColorCreate_VertexArray2");

				GL.EnableVertexAttribArray(2);
				GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, true, Vertex.Size, 32);
				Program.CheckGraphicsError("ColorCreate_VertexArray3");
			}
		}

		public override void Bind()
		{
			lock (MasterRenderer.GLLock)
			{
				UseProgram();
				GL.BindVertexArray(VertexArrayID);
				Program.CheckGraphicsError("Color_Bind");
				GL.BindTexture(TextureTarget.Texture2D, 0);
				Program.CheckGraphicsError("ImageBind_Texture");
			}
		}

		public override void Render()
		{
			lock (MasterRenderer.GLLock)
			{
				GL.DrawArrays(PrimitiveType.Triangles, 0, VerticeCount);
				Program.CheckGraphicsError("Color_Draw");
			}
		}
	}
}