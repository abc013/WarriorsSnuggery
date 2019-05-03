using System;
using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery.Graphics
{
	public class IColor : Renderable
	{
		readonly PrimitiveType renderType;

		public IColor(ColoredVertex[] vertices, DrawMethod type = DrawMethod.TRIANGLE) : base(MasterRenderer.ColorShader, vertices.Length)
		{
			renderType = (PrimitiveType) type;

			lock(MasterRenderer.GLLock)
			{
				GL.BufferData(BufferTarget.ArrayBuffer, ColoredVertex.Size * vertices.Length, vertices, BufferUsageHint.StaticDraw);
				Program.CheckGraphicsError("ColorCreate_Buffer");

				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, true, ColoredVertex.Size, 0);
				Program.CheckGraphicsError("ColorCreate_VertexArray1");

				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, true, ColoredVertex.Size, 16);
				Program.CheckGraphicsError("ColorCreate_VertexArray2");
			}
		}

		public override void Bind()
		{
			lock(MasterRenderer.GLLock)
			{
				UseProgram();
				GL.BindVertexArray(VertexArrayID);
				Program.CheckGraphicsError("Color_Bind");
			}
		}

		public override void Render()
		{
			lock(MasterRenderer.GLLock)
			{
				GL.DrawArrays(renderType, 0, VerticeCount);
				Program.CheckGraphicsError("Color_Draw");
			}
		}
	}
}