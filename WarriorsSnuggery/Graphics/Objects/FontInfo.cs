namespace WarriorsSnuggery.Graphics
{
	public class FontInfo
	{
		public readonly int Size;
		public readonly string FontName;
		public MPos MaxSize;
		public MPos SpaceSize;
		public MPos[] CharSizes;

		public FontInfo(int size, string fontName)
		{
			Size = size;
			FontName = fontName;
		}
	}
}
