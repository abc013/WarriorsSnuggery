using System;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Graphics
{
	public static class SpriteManager
	{
		public static Sheet[] Sheets { get; private set; }

		public static int SheetsUsed => currentSheet;

		static int currentSheet;
		static bool sheetsLoaded;

		public static void InitSheets()
		{
			Sheets = new Sheet[Settings.MaxSheets];

			nextSheet();
		}

		static void nextSheet()
		{
			if (currentSheet >= Sheets.Length)
				throw new OverflowException($"Tried to create new Sheet with index {currentSheet} (Max allowed: {Sheets.Length}). Try increasing the max sheet count.");

			Sheets[currentSheet] = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(Sheets[currentSheet]);

			currentSheet++;
		}

		public static Texture[] AddTexture(TextureInfo info)
		{
			if (sheetsLoaded)
				throw new Exception($"Unable to add texture (file: {info.File}. Sheets are already loaded.");

			if (info.Type == TextureType.IMAGE)
			{
				var data = BitmapLoader.LoadTexture(info.File, out var w, out var h);
				info = new TextureInfo(info.File, info.Type, 0, w, h, false, false);

				return new[] { addTexture(data, info) };
			}

			var dataList = BitmapLoader.LoadSprite(info.File, info.Width, info.Height);

			var textures = new Texture[dataList.Count];
			for (int i = 0; i < dataList.Count; i++)
				textures[i] = addTexture(dataList[i], info);

			return textures;
		}

		public static Texture[] AddFont(FontInfo font)
		{
			if (sheetsLoaded)
				throw new Exception($"Unable to add font (name: {font.FontName}). Sheets are already loaded.");

			var data = FontLoader.LoadCharacters(font);
			var textures = new Texture[data.Length];

			for (int i = 0; i < data.Length; i++)
			{
				var info = new TextureInfo(font.FontName, TextureType.IMAGE, 0, font.CharSizes[i], false, false);

				textures[i] = addTexture(data[i], info);
			}

			return textures;
		}

		static Texture addTexture(float[] data, TextureInfo info)
		{
			if (!SheetBuilder.HasSpaceLeft(info.Width, info.Height))
				nextSheet();

			return SheetBuilder.WriteTexture(data, info);
		}

		public static int SheetIndex(int SheetID)
		{
			for (int i = 0; i < Sheets.Length; i++)
				if (Sheets[i] != null && Sheets[i].TextureID == SheetID)
					return i;

			return 0;
		}

		public static void FinishSheets()
		{
			if (Settings.DeveloperMode)
			{
				for (int i = 0; i < Sheets.Length; i++)
				{
					var sheet = Sheets[i];
					if (sheet == null)
						break;

					BitmapSaver.Save($"{FileExplorer.Logs}spritesheet_{i}.png", TextureManager.GetContent(sheet.TextureID, sheet.Size, sheet.Size), sheet.Bounds);
				}
			}

			SheetBuilder.Clear();

			sheetsLoaded = true;
		}

		public static void DeleteSheets()
		{
			foreach (var sheet in Sheets)
				sheet?.Dispose();
		}
	}
}
