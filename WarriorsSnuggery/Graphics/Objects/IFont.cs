using System.Drawing.Text;

namespace WarriorsSnuggery.Graphics
{
	public class IFont
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
		}

		public static void InitializeFonts()
		{
			Collection.AddFontFile(FileExplorer.Misc + @"Fonts\PAPYRUS.ttf");
			Papyrus24 = new IFont(new FontInfo(24, "Papyrus"));

			Collection.AddFontFile(FileExplorer.Misc + @"Fonts\Pixel.ttf");
			Pixel16 = new IFont(new FontInfo(12, "Pixel"));
		}

		public float PixelMultiplier
		{
			get { return MasterRenderer.PixelMultiplier * 6f / Info.Size; }
		}

		public int Gap
		{
			get { return (int)(1024 * Info.SpaceSize.X * MasterRenderer.PixelMultiplier); }
		}

		public int Width
		{
			get { return (int)(1024 * Info.MaxSize.X * MasterRenderer.PixelMultiplier); }
		}

		public int Height
		{
			get { return (int)(1024 * Info.MaxSize.Y * MasterRenderer.PixelMultiplier); }
		}

		readonly ITexture[] characters;
		public readonly FontInfo Info;

		public IFont(FontInfo info)
		{
			Info = info;
			characters = SpriteManager.AddFont(info);
			Info.SpaceSize = new MPos((int)(Info.MaxSize.X * 0.8f), Info.MaxSize.Y);
		}

		public int GetWidth(char c)
		{
			return Info.CharSizes[TextureManager.Characters.IndexOf(c)].X;
		}

		public ITexture GetTexture(char c)
		{
			return characters[TextureManager.Characters.IndexOf(c)];
		}
	}
}
