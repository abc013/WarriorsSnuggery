namespace WarriorsSnuggery.Graphics
{
	public class TerrainSpriteManager
	{
		public static Sheet sheet;

		public static void CreateSheet()
		{
			sheet = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(sheet);
		}

		public static IImage[] AddTexture(TextureInfo info)
		{
			var data = TextureManager.loadSprite(info.File, info.Width, info.Height);
			var renderables = new IImage[data.Length];

			for (int i = 0; i < data.Length; i++)
			{
				var texture = SheetBuilder.WriteTexture(data[i], info);
				var renderable = IImage.Create(TexturedMesh.Terrain(texture.Offset.X, texture.Offset.Y, texture.Width, texture.Height), texture);

				renderables[i] = renderable;
			}

			return renderables;
		}

		public static void CreateTexture()
		{
			Loader.BitmapSaver.Save(FileExplorer.Logs + "terrainSheet.png", sheet.Data, sheet.Size);
			sheet.CreateTexture(true);
			SheetBuilder.Clear();
		}

		public static void DeleteTexture()
		{
			sheet.Dispose();
		}
	}
}
