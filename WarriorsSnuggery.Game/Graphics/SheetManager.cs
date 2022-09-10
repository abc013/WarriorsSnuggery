using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Graphics
{
	public static class SheetManager
	{
		static readonly Dictionary<int, Texture> hashedTextures = new Dictionary<int, Texture>();

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

		public static Texture[] AddTexture(string filepath, out int width, out int height)
		{
			if (sheetsLoaded)
				throw new Exception($"Unable to add texture (file: {filepath}. Sheets are already loaded.");

			var data = BitmapLoader.LoadWhole(filepath, out width, out height);

			return new[] { addTexture(data, filepath, width, height) };
		}

		public static Texture[] AddSprite(string filepath, int width, int height)
		{
			if (sheetsLoaded)
				throw new Exception($"Unable to add texture (file: {filepath}. Sheets are already loaded.");

			var dataList = BitmapLoader.LoadSplit(filepath, width, height);

			var textures = new Texture[dataList.Count];
			for (int i = 0; i < dataList.Count; i++)
				textures[i] = addTexture(dataList[i], filepath + i, width, height);

			return textures;
		}

		public static Texture[] AddFont(FontInfo font)
		{
			if (sheetsLoaded)
				throw new Exception($"Unable to add font (name: {font.FontName}). Sheets are already loaded.");

			var data = FontLoader.LoadCharacters(font);
			var textures = new Texture[data.Length];

			for (int i = 0; i < data.Length; i++)
				textures[i] = addTexture(data[i], font.FontName + i, font.CharSizes[i].X, font.CharSizes[i].Y);

			return textures;
		}

		static Texture addTexture(in float[] data, string filepath, int width, int height)
		{
			var hash = filepath.GetHashCode() ^ width ^ height;

			if (hashedTextures.ContainsKey(hash))
				return hashedTextures[hash];

			if (!SheetBuilder.HasSpaceLeft(width, height))
				nextSheet();

			var texture = SheetBuilder.WriteTexture(data, filepath, width, height);

			hashedTextures.Add(hash, texture);

			return texture;
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
			hashedTextures.Clear();

			sheetsLoaded = true;
		}

		public static void DeleteSheets()
		{
			foreach (var sheet in Sheets)
				sheet?.Dispose();
		}
	}
}
