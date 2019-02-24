using System;
using System.Drawing.Text;
using OpenTK.Graphics.ES30;
using OpenTK.Graphics;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public class IFont : Renderable
	{
		//public static IFont Arial18 { get; private set; }
		public static IFont Papyrus24 { get; private set; }
		public static IFont Pixel16 { get; private set; }
		public static PrivateFontCollection Collection;

		public static void LoadFonts()
		{
			Collection = new PrivateFontCollection();
		}

		public static void DisposeFonts()
		{
			Collection.Dispose();
			Papyrus24.Dispose();
			Pixel16.Dispose();
		}

		public static void InitializeFonts()
		{
			//Arial18 = new IFont("Arial", 18);
			Collection.AddFontFile(FileExplorer.Misc + @"Fonts\PAPYRUS.TTF");
			Papyrus24 = new IFont("Papyrus", 24);
			Collection.AddFontFile(FileExplorer.Misc + @"Fonts\Pixel.ttf");
			Pixel16 = new IFont("Pixel", 16);
		}

		public readonly ITexture Font;
		public readonly MPos MaxSize;
		public readonly int[] CharWidth;
		public readonly TexturedVertex[] Mesh;

		public IFont(string fontname, int size) : base(MasterRenderer.FontShader, vertices.Length)
		{
			IImage.CreateTextureBuffer(vertices, BufferID, VertexArrayID);

			Font = TextureManager.Font(fontname, size, out MaxSize, out CharWidth);
			Mesh = TexturedMesh.Character(this);
		}

		// Note: in case of rendering the whole font.
		static readonly TexturedVertex[] vertices = TexturedMesh.Plane(0.01f, 3432, 48);

		public override void Bind()
		{
			lock(MasterRenderer.GLLock)
			{
				UseProgram();
				GL.VertexAttrib2(GL.GetAttribLocation(MasterRenderer.FontShader, "textureOffset"), OpenTK.Vector2.Zero);

				var color = Color.White.toColor4();
				GL.VertexAttrib4(GL.GetAttribLocation(MasterRenderer.FontShader, "color"), color.R, color.G, color.B, color.A);

				GL.BindVertexArray(VertexArrayID);
				GL.BindTexture(TextureTarget.Texture2D, Font.ID);
			}
		}
	}
}
