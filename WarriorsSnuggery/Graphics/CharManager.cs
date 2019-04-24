using System;

namespace WarriorsSnuggery.Graphics
{
	public static class CharManager
	{
		public static IChar Pixel16;
		public static IChar Papyrus24;

		public static void Initialize()
		{
			Pixel16 = new IChar(IFont.Pixel16);
			Papyrus24 = new IChar(IFont.Papyrus24);
		}

		public static void Dispose()
		{
			Pixel16.Dispose();
			Papyrus24.Dispose();
		}
	}
}
