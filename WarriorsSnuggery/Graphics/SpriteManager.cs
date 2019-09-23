namespace WarriorsSnuggery.Graphics
{
	public class SpriteManager
	{
		public static Sheet[] sheets;
		static int currentSheet;

		public static void CreateSheet(int maxSheets)
		{
			sheets = new Sheet[maxSheets];
			currentSheet = 0;

			sheets[currentSheet] = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(sheets[currentSheet]);
		}

		public static void UseNextSheet()
		{
			currentSheet++;

			if (currentSheet >= sheets.Length)
				throw new System.Exception("Sheetoverflow.");

			sheets[currentSheet] = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(sheets[currentSheet]);
		}

		public static IImage[] AddTexture(TextureInfo info)
		{
			// TODO cache textureInfos and lookup
			var data = TextureManager.loadSprite(info.File, info.Width, info.Height);
			var renderables = new IImage[data.Length];

			for (int i = 0; i < data.Length; i++)
			{
				if (!SheetBuilder.IsSpaceLeft(info.Width, info.Height))
					UseNextSheet();

				var texture = SheetBuilder.WriteTexture(data[i], info);
				var renderable = IImage.Create(TexturedMesh.Terrain(texture.Offset.X, texture.Offset.Y, texture.Width, texture.Height), texture);

				renderables[i] = renderable;
			}

			return renderables;
		}

		public static void CreateTextures()
		{
			int i = 0;
			foreach (var sheet in sheets)
			{
				if (sheet == null)
					continue;

				Loader.BitmapSaver.Save(FileExplorer.Logs + "spritesheet_" + i + ".png", sheet.Data, sheet.Size);
				sheet.CreateTexture(true);
				i++;
			}
			SheetBuilder.Clear();
		}

		public static void DeleteTextures()
		{
			foreach (var sheet in sheets)
				sheet?.Dispose();
		}
	}
}
