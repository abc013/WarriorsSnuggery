using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public class SpriteManager
	{
		public static Sheet[] sheets;
		static int currentSheet;

		static readonly Dictionary<int, ITexture[]> hashedTextures = new Dictionary<int, ITexture[]>();

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

		public static ITexture[] AddTexture(TextureInfo info)
		{
			var hash = info.GetHashCode();

			if (hashedTextures.ContainsKey(hash))
				return hashedTextures[hash];

			var data = TextureManager.loadSprite(info.File, info.Width, info.Height);
			var textures = new ITexture[data.Length];

			for (int i = 0; i < data.Length; i++)
			{
				if (!SheetBuilder.IsSpaceLeft(info.Width, info.Height))
					UseNextSheet();

				textures[i] = SheetBuilder.WriteTexture(data[i], info);
			}

			hashedTextures.Add(hash, textures);

			return textures;
		}

		public static ITexture[] AddFont(FontInfo font)
		{
			var data = TextureManager.LoadCharacters(font.Size, font.FontName, out font.MaxSize, out font.CharSizes);
			var textures = new ITexture[data.Length];

			for (int i = 0; i < data.Length; i++)
			{
				if (!SheetBuilder.IsSpaceLeft(font.CharSizes[i].X, font.CharSizes[i].Y))
					UseNextSheet();

				textures[i] = SheetBuilder.WriteTexture(data[i], new TextureInfo(font.FontName, TextureType.IMAGE, 0, font.CharSizes[i].X, font.CharSizes[i].Y, false));
			}

			return textures;
		}

		public static ITexture[] GetTexture(TextureInfo info)
		{
			return hashedTextures[info.GetHashCode()];
		}

		public static int SheetIndex(int SheetID)
		{
			for (int i = 0; i < sheets.Length; i++)
				if (sheets[i] != null && sheets[i].TextureID == SheetID)
					return i;

			return 0;
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
