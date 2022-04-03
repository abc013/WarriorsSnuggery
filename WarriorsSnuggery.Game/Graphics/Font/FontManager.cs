using SixLabors.Fonts;

namespace WarriorsSnuggery.Graphics
{
	public static class FontManager
	{
		public static Font Header { get; private set; }
		public static Font Default { get; private set; }

		public static FontCollection Collection { get; private set; }

		public const char UnknownCharacter = '?';
		public const string Characters = @" qwertyuiopasdfghjklzxcvbnmäöüQWERTYUIOPASDFGHJKLZXCVBNMÄÖÜ0123456789µ§!""#%&/()=?^*@${[]}\~¨'¯-_.:,;<>|°+↓↑←→∞";

		public static void Load()
		{
			Collection = new FontCollection();

			Collection.Add(FileExplorer.Fonts + "Adventurer.ttf");
			Header = new Font(new FontInfo(16, "Adventurer"));

			Collection.Add(FileExplorer.Fonts + "QuinqueFive.ttf");
			Default = new Font(new FontInfo(5, "QuinqueFive"));
		}
	}
}
