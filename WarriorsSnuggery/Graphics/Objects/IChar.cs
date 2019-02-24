﻿using System;
using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class IChar : Renderable
	{
		public readonly IFont Font;
		public const string Characters = TextureManager.Characters;

		public int offset { get; private set; }

		public Color color { get; private set; } // TODO: aim is to replace all chars with a single char object which then uses the color space and the offset to render the char.

		public IChar(IFont font, char @char, Color color, TexturedVertex[] vertices/* already replaced*/) : base(MasterRenderer.FontShader, vertices.Length)
		{
			IImage.CreateTextureBuffer(vertices, BufferID, VertexArrayID);

			offset = Characters.IndexOf(@char);
			Font = font;
			this.color = color;
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
					GL.VertexAttrib4(2, color.toVector4());
					GL.VertexAttrib4(3, new OpenTK.Vector4(offset * Font.MaxSize.X, 0,0,0));
					GL.BindVertexArray(VertexArrayID);
					GL.BindTexture(TextureTarget.Texture2D, Font.Font.ID);
				}
			}
		}

		public override void Render()
		{
			if (offset > 0) base.Render();
		}
	}
}
