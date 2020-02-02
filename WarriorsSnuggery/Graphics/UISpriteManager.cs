namespace WarriorsSnuggery.Graphics
{
	public class UISpriteManager
	{
		public static Sheet sheet;

		public static void CreateSheet()
		{
			sheet = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(sheet);
		}

		public static ITexture[] AddTexture(TextureInfo info)
		{
			var data = TextureManager.loadSprite(info.File, info.Width, info.Height);
			var textures = new ITexture[data.Length];

			for (int i = 0; i < data.Length; i++)
				textures[i] = SheetBuilder.WriteTexture(data[i], info);

			return textures;
		}

		public static void CreateTexture()
		{
			Loader.BitmapSaver.Save(FileExplorer.Logs + "UISheet.png", sheet.Data, sheet.Size);
			sheet.CreateTexture(true);
			SheetBuilder.Clear();
		}

		public static void DeleteTexture()
		{
			sheet.Dispose();
		}
	}
}
