namespace WarriorsSnuggery.Graphics
{
	public class Sheet
	{
		public readonly int Size;
		public MPos Bounds => new MPos(Size, Size);

		public readonly int TextureID;

		public Sheet(int size)
		{
			Size = size;
			TextureID = TextureManager.Create(size, size);
		}

		public void WriteData(float[] data, int offsetx, int offsety, int width, int height)
		{
			TextureManager.Write(TextureID, data, offsetx, offsety, width, height);
		}

		public void Dispose()
		{
			TextureManager.Dispose(TextureID);
		}
	}
}
