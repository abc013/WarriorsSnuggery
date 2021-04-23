namespace WarriorsSnuggery.Graphics
{
	public class Texture
	{
		public readonly int SheetID;

		public readonly int X;
		public readonly int Y;
		public readonly int Width;
		public readonly int Height;

		public Texture(int x, int y, int width, int height, int sheetID)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
			SheetID = sheetID;
		}
	}
}
