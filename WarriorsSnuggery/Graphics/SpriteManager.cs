using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public class SpriteManager
	{
		public static Sheet[] sheets;
		public static int CurrentSheet;

		static readonly Dictionary<int, Texture[]> hashedTextures = new Dictionary<int, Texture[]>();

		public static void CreateSheet(int maxSheets)
		{
			sheets = new Sheet[maxSheets];
			CurrentSheet = 0;

			sheets[CurrentSheet] = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(sheets[CurrentSheet]);
		}

		public static void UseNextSheet()
		{
			CurrentSheet++;

			if (CurrentSheet >= sheets.Length)
				throw new System.Exception("Sheetoverflow.");

			sheets[CurrentSheet] = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(sheets[CurrentSheet]);
		}

		public static Texture[] AddTexture(TextureInfo info)
		{
			var hash = info.GetHashCode();

			if (hashedTextures.ContainsKey(hash))
				return hashedTextures[hash];

			float[][] data;
			if (info.Type == TextureType.IMAGE)
			{
				data = new[] { TextureManager.LoadTexture(info.File, out var w, out var h) };
				info = new TextureInfo(info.File, info.Type, 0, w, h, false);
			}
			else
				data = TextureManager.LoadSprite(info.File, info.Width, info.Height);

			var textures = new Texture[data.Length];

			for (int i = 0; i < data.Length; i++)
			{
				if (!SheetBuilder.IsSpaceLeft(info.Width, info.Height))
					UseNextSheet();

				textures[i] = SheetBuilder.WriteTexture(data[i], info);
			}

			hashedTextures.Add(hash, textures);

			return textures;
		}

		public static Texture[] AddFont(FontInfo font)
		{
			var data = TextureManager.LoadCharacters(font.Size, font.FontName, out font.MaxSize, out font.CharSizes);
			var textures = new Texture[data.Length];

			for (int i = 0; i < data.Length; i++)
			{
				if (!SheetBuilder.IsSpaceLeft(font.CharSizes[i].X, font.CharSizes[i].Y))
					UseNextSheet();

				textures[i] = SheetBuilder.WriteTexture(data[i], new TextureInfo(font.FontName, TextureType.IMAGE, 0, font.CharSizes[i].X, font.CharSizes[i].Y, false));
			}

			return textures;
		}

		public static Texture[] GetTexture(TextureInfo info)
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
