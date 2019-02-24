/*
 * User: Andreas
 * Date: 09.08.2017
 * 
 */
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

				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, true, ColoredVertex.Size, 0);

				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, true, ColoredVertex.Size, 16);
			}
		}

		public override void Bind()
		{
			lock(MasterRenderer.GLLock)
			{
				UseProgram();
				GL.BindVertexArray(VertexArrayID);
			}
		}

		public override void Render()
		{
			lock(MasterRenderer.GLLock)
			{
				GL.DrawArrays(renderType, 0, VerticeCount);
			}
		}
	}
}