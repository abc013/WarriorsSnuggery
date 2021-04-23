namespace WarriorsSnuggery.Graphics
{
	public class Sheet
	{
		public readonly MPos Size;
		public readonly int TextureID;

		public Sheet(int size)
		{
			Size = new MPos(size, size);
			TextureID = TextureManager.Create(Size);
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
