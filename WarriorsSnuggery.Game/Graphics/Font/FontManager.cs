using System.Drawing.Text;

namespace WarriorsSnuggery.Graphics
{
	public static class FontManager
	{
		public static Font Papyrus24 { get; private set; }
		public static Font Pixel16 { get; private set; }

		public static PrivateFontCollection Collection { get; private set; }

		public const char UnknownCharacter = '?';
		public const string Characters = @" qwertyuiopasdfghjklzxcvbnmäöüQWERTYUIOPASDFGHJKLZXCVBNMÄÖÜ0123456789µ§!""#%&/()=?^*@${[]}\~¨'¯-_.:,;<>|°+↓↑←→∞";

		public static void Load()
		{
			Collection = new PrivateFontCollection();

			Collection.AddFontFile(FileExplorer.Misc + @"Fonts\Papyrus.ttf");
			Papyrus24 = new Font(new FontInfo(24, "Papyrus"));

			Collection.AddFontFile(FileExplorer.Misc + @"Fonts\Pixel.ttf");
			Pixel16 = new Font(new FontInfo(16, "Pixel"));
		}

		public static void Dispose()
		{
			Collection.Dispose();
		}
	}
}
