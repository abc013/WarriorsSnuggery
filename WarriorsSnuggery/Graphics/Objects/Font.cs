using System.Drawing.Text;

namespace WarriorsSnuggery.Graphics
{
	public class Font
	{
		public static Font Papyrus24 { get; private set; }
		public static Font Pixel16 { get; private set; }

		public static PrivateFontCollection Collection { get; private set; }

		public const string Characters = @" qwertyuiopasdfghjklzxcvbnmäöüQWERTYUIOPASDFGHJKLZXCVBNMÄÖÜ0123456789µ§!""#%&/()=?^*@${[]}\~¨'¯-_.:,;<>|°+↓↑←→∞";

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
			Papyrus24 = new Font(new FontInfo(24, "Papyrus"));

			Collection.AddFontFile(FileExplorer.Misc + @"Fonts\Pixel.ttf");
			Pixel16 = new Font(new FontInfo(12, "Pixel"));
		}

		public float PixelMultiplier
		{
			get { return MasterRenderer.PixelMultiplier * 2.5f / Info.Size * WindowInfo.Ratio; }
		}

		public int Gap
		{
			get { return (int)(10 * Info.SpaceSize.Y * PixelMultiplier); }
		}

		public int Width
		{
			get { return (int)(512 * Info.MaxSize.X * PixelMultiplier); }
		}

		public int Height
		{
			get { return (int)(1024 * Info.MaxSize.Y * PixelMultiplier); }
		}

		readonly ITexture[] characters;
		public readonly FontInfo Info;

		public Font(FontInfo info)
		{
			Info = info;
			characters = SpriteManager.AddFont(info);
			Info.SpaceSize = new MPos((int)(Info.MaxSize.X * 0.8f), Info.MaxSize.Y);
		}

		public int GetWidth(char c)
		{
			return Info.CharSizes[Characters.IndexOf(c)].X;
		}

		public ITexture GetTexture(char c)
		{
			return characters[Characters.IndexOf(c)];
		}
	}
}
