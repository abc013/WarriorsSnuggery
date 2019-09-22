namespace WarriorsSnuggery.Graphics
{
	public class Sheet
	{
		public readonly MPos Size;

		public int TextureID { get; private set; }

		public float[] Data;

		public Sheet(int size)
		{
			Size = new MPos(size, size);
			Data = new float[size * size * 4];
			createTexture();
		}

		void createTexture()
		{
			TextureID = TextureManager.Create(Size);
		}

		public void CreateTexture(bool final)
		{
			TextureManager.Write(TextureID, Data, Size);

			if (final)
				Data = null;
		}

		public void Dispose()
		{
			TextureManager.Dispose(TextureID);
		}
	}
}
