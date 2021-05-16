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
			var path = FileExplorer.Misc + "fonts" + FileExplorer.Separator;

			Collection = new PrivateFontCollection();

			Collection.AddFontFile(path + "Adventurer.ttf");
			Papyrus24 = new Font(new FontInfo(24, "Adventurer"));

			Collection.AddFontFile(path + "Pixel.ttf");
			Pixel16 = new Font(new FontInfo(16, "Pixel"));
		}

		public static void Dispose()
		{
			Collection.Dispose();
		}
	}
}
