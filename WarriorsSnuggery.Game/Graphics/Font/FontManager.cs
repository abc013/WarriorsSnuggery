﻿using SixLabors.Fonts;

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
			var path = FileExplorer.Misc + "fonts" + FileExplorer.Separator;

			Collection = new FontCollection();

			Collection.Install(path + "Adventurer.ttf");
			Header = new Font(new FontInfo(16, "Adventurer"));

			Collection.Install(path + "Pixel.ttf");
			Default = new Font(new FontInfo(16, "Pixel"));
		}
	}
}
