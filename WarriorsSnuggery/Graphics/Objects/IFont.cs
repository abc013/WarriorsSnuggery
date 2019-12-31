using OpenTK.Graphics.ES30;
using System.Drawing.Text;

namespace WarriorsSnuggery.Graphics
{
	public class IFont : Renderable
	{
		public static IFont Papyrus24 { get; private set; }
		public static IFont Pixel16 { get; private set; }

		public static PrivateFontCollection Collection { get; private set; }

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
			Collection.AddFontFile(FileExplorer.Misc + @"Fonts\PAPYRUS.TTF");
			Papyrus24 = new IFont("Papyrus", 24);

			Collection.AddFontFile(FileExplorer.Misc + @"Fonts\Pixel.ttf");
			Pixel16 = new IFont("Pixel", 16);
		}

		public const float FontSizeMultiplier = MasterRenderer.PixelMultiplier / 4;
		public int Gap
		{
			get { return 1 * 128 / MasterRenderer.PixelSize; }
			set { }
		}
		public int Width
		{
			get { return MaxSize.X * 512 / MasterRenderer.PixelSize; }
			set { }
		}
		public int Height
		{
			get { return MaxSize.X * 256 / MasterRenderer.PixelSize; }
			set { }
		}

		public readonly ITexture Font;
		public readonly MPos MaxSize;
		public readonly MPos SpaceSize;
		public readonly CharVertex[] Vertices;

		readonly int[] charWidths;

		public IFont(string fontname, int size) : base(MasterRenderer.FontShader, vertices.Length)
		{
			IImage.CreateTextureBuffer(vertices);

			Font = TextureManager.Font(fontname, size, out MaxSize, out charWidths);
			SpaceSize = new MPos(MaxSize.X / 2, MaxSize.Y);

			Vertices = CharMesh.Character(this);
		}

		public int getCharWidth(char c)
		{
			return charWidths[TextureManager.Characters.IndexOf(c)];
		}

		// Note: in case of rendering the whole font.
		static readonly Vertex[] vertices = Mesh.Plane(0.01f, 3432, 48, Color.White);

		public override void Bind()
		{
			lock (MasterRenderer.GLLock)
			{
				UseProgram();
				GL.VertexAttrib2(GL.GetAttribLocation(MasterRenderer.FontShader, "textureOffset"), OpenTK.Vector2.Zero);
				Program.CheckGraphicsError("FontBind_Offset");

				var color = Color.White.toColor4();
				GL.VertexAttrib4(GL.GetAttribLocation(MasterRenderer.FontShader, "color"), color.R, color.G, color.B, color.A);
				Program.CheckGraphicsError("FontBind_Color");

				GL.BindVertexArray(VertexArrayID);
				Program.CheckGraphicsError("FontBind_Array");
				GL.BindTexture(TextureTarget.Texture2D, Font.SheetID);
				Program.CheckGraphicsError("FontBind_Texture");
			}
		}
	}
}
