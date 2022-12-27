using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace WarriorsSnuggery.Graphics
{
	public class TexturedRenderable : Renderable
	{
		readonly Texture texture;

		public TexturedRenderable(Vertex[] vertices, Texture texture) : base(Graphics.Shader.TextureShader, vertices.Length)
		{
			this.texture = texture;

			var handle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
			try
			{
				TextureBuffer.ConfigureBugger(VertexArrayID, BufferID, Vertex.Size * vertices.Length, handle.AddrOfPinnedObject(), BufferUsageHint.StaticDraw);
			}
			finally
			{
				if (handle.IsAllocated)
					handle.Free();
			}
		}

		public override void Bind()
		{
			lock (MasterRenderer.GLLock)
			{
				GL.UseProgram(Shader.ID);
				GL.BindVertexArray(VertexArrayID);
				GL.BindTexture(TextureTarget.Texture2D, texture.SheetID);
				Program.CheckGraphicsError("Image_Bind");
			}
		}
	}

	internal static class TextureBuffer
	{
		public static void ConfigureBugger(int vertexarrayID, int bufferID, int size, IntPtr data, BufferUsageHint hint)
		{
			lock (MasterRenderer.GLLock)
			{
				GL.BindVertexArray(vertexarrayID);
				GL.BindBuffer(BufferTarget.ArrayBuffer, bufferID);
				Program.CheckGraphicsError("Buffer_Configuration_1");

				GL.BufferData(BufferTarget.ArrayBuffer, size, data, hint);
				Program.CheckGraphicsError("Buffer_Configuration_2");

				GL.EnableVertexAttribArray(Vertex.PositionAttributeLocation);
				GL.VertexAttribPointer(Vertex.PositionAttributeLocation, 3, VertexAttribPointerType.Float, true, Vertex.Size, 0);
				Program.CheckGraphicsError("Buffer_Configuration_3");

				GL.EnableVertexAttribArray(Vertex.TextureCoordinateAttributeLocation);
				GL.VertexAttribPointer(Vertex.TextureCoordinateAttributeLocation, 2, VertexAttribPointerType.Float, true, Vertex.Size, 12);
				Program.CheckGraphicsError("Buffer_Configuration_4");

				GL.EnableVertexAttribArray(Vertex.TextureAttributeLocation);
				GL.VertexAttribIPointer(Vertex.TextureAttributeLocation, 1, VertexAttribIntegerType.Int, Vertex.Size, (IntPtr)20);
				Program.CheckGraphicsError("Buffer_Configuration_5");

				GL.EnableVertexAttribArray(Vertex.TextureFlagsAttributeLocation);
				GL.VertexAttribIPointer(Vertex.TextureFlagsAttributeLocation, 1, VertexAttribIntegerType.Int, Vertex.Size, (IntPtr)24);
				Program.CheckGraphicsError("Buffer_Configuration_6");

				GL.EnableVertexAttribArray(Vertex.ColorAttributeLocation);
				GL.VertexAttribPointer(Vertex.ColorAttributeLocation, 4, VertexAttribPointerType.Float, true, Vertex.Size, 28);
				Program.CheckGraphicsError("Buffer_Configuration_7");
			}
		}
	}
}
