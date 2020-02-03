using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class IChar : Renderable
	{
		public readonly IFont Font;
		public const string Characters = TextureManager.Characters;

		public int offset { get; private set; }

		public Color color { get; private set; }

		public IChar(IFont font) : base(MasterRenderer.FontShader, font.Vertices.Length)
		{
			createBuffer(font.Vertices);

			offset = Characters.IndexOf(' ');
			color = Color.White;
			Font = font;
		}

		void createBuffer(Vertex[] vertices)
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
				Program.CheckGraphicsError("CreateTexture_VertexArray2");
			}
		}

		public void SetColor(Color color)
		{
			this.color = color;
		}

		public void SetChar(char @char)
		{
			offset = Characters.IndexOf(@char);
		}

		public override void Bind()
		{
			if (offset > 0)
			{
				lock (MasterRenderer.GLLock)
				{
					GL.UseProgram(ProgramID);
					Program.CheckGraphicsError("CharBind_UseProgram");
					GL.VertexAttrib4(3, new OpenTK.Vector4(offset * Font.MaxSize.X, 0, 0, 0));
					Program.CheckGraphicsError("CharBind_OffsetAttrib");
					GL.BindVertexArray(VertexArrayID);
					Program.CheckGraphicsError("CharBind_VertexArray");
					GL.BindTexture(TextureTarget.Texture2D, Font.Font.SheetID);
					Program.CheckGraphicsError("CharBind_Texture");
				}
			}
		}

		public override void Render()
		{
			if (offset > 0) base.Render();
		}
	}
}
