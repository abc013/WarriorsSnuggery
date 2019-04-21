using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarriorsSnuggery.Graphics
{
	public static class CharManager
	{
		public static IChar Pixel16;
		public static IChar Papyrus24;

		public static void Initialize()
		{
			Pixel16 = new IChar(IFont.Pixel16, ' ', Color.White, IFont.Pixel16.Mesh);
			Papyrus24 = new IChar(IFont.Papyrus24, ' ', Color.White, IFont.Papyrus24.Mesh);
		}

		public static void Dispose()
		{
			Pixel16.Dispose();
			Papyrus24.Dispose();
		}
	}
}
