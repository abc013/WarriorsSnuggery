namespace WarriorsSnuggery.Graphics
{
	public class FontInfo
	{
		public readonly int Size;
		public readonly string FontName;

		public MPos MaxSize { get; private set; }
		public MPos SpaceSize { get; private set; }
		public MPos[] CharSizes { get; private set; }

		public FontInfo(int size, string fontName)
		{
			Size = size;
			FontName = fontName;
		}

		public void SetSizes(MPos maxSize, MPos[] charSizes)
		{
			MaxSize = maxSize;
			CharSizes = charSizes;
			SpaceSize = new MPos(MaxSize.X * 4 / 5, MaxSize.Y);
		}
	}
}
