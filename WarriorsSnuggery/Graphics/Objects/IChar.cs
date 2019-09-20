using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class IChar : Renderable
	{
		public readonly IFont Font;
		public const string Characters = TextureManager.Characters;

		public int offset { get; private set; }

		public Color color { get; private set; }

		public IChar(IFont font) : base(MasterRenderer.FontShader, font.Mesh.Length)
		{
			IImage.CreateTextureBuffer(font.Mesh, BufferID, VertexArrayID);

			offset = Characters.IndexOf(' ');
			color = Color.White;
			Font = font;
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
					GL.VertexAttrib4(2, color.toVector4());
					Program.CheckGraphicsError("CharBind_ColorAttrib");
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
