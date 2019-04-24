/*
 * User: Andreas
 * Date: 03.11.2017
 * 
 */
using System;
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
					//var x = GL.IsVertexArray(VertexArrayID);
					//if (!x)
					//	return; // TODO HACK
					GL.UseProgram(ProgramID);
					GL.VertexAttrib4(2, color.toVector4());
					GL.VertexAttrib4(3, new OpenTK.Vector4(offset * Font.MaxSize.X, 0,0,0));
					GL.BindVertexArray(0);
					GL.BindVertexArray(VertexArrayID);
					GL.BindTexture(TextureTarget.Texture2D, Font.Font.ID);
					Program.CheckGraphicsError("renderText");
				}
			}
		}

		public override void Render()
		{
			//var x = GL.IsVertexArray(VertexArrayID);
			//if (!x)
			//	return;
			if (offset > 0) base.Render();
		}
	}
}
