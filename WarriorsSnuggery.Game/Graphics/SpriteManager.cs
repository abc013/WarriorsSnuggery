﻿using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public static class SpriteManager
	{
		public static Sheet[] Sheets;
		public static int CurrentSheet;

		static readonly Dictionary<int, Texture[]> hashedTextures = new Dictionary<int, Texture[]>();

		public static void InitSheets()
		{
			Sheets = new Sheet[Settings.MaxSheets];
			CurrentSheet = 0;

			Sheets[CurrentSheet] = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(Sheets[CurrentSheet]);
		}

		static void nextSheet()
		{
			CurrentSheet++;

			if (CurrentSheet >= Sheets.Length)
				throw new System.Exception("Sheetoverflow.");

			Sheets[CurrentSheet] = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(Sheets[CurrentSheet]);
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
					nextSheet();

				textures[i] = SheetBuilder.WriteTexture(data[i], info);
			}

			hashedTextures.Add(hash, textures);

			return textures;
		}

		public static Texture[] AddFont(FontInfo font)
		{
			var data = TextureManager.LoadCharacters(font);
			var textures = new Texture[data.Length];

			for (int i = 0; i < data.Length; i++)
			{
				if (!SheetBuilder.IsSpaceLeft(font.CharSizes[i].X, font.CharSizes[i].Y))
					nextSheet();

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
			for (int i = 0; i < Sheets.Length; i++)
				if (Sheets[i] != null && Sheets[i].TextureID == SheetID)
					return i;

			return 0;
		}

		public static void CreateTextures()
		{
			int i = 0;
			foreach (var sheet in Sheets)
			{
				if (sheet == null)
					continue;

				Loader.BitmapSaver.Save(FileExplorer.Logs + "spritesheet_" + i + ".png", sheet.Data, sheet.Size);
				sheet.CreateTexture();
				i++;
			}
			SheetBuilder.Clear();
		}

		public static void DeleteTextures()
		{
			foreach (var sheet in Sheets)
				sheet?.Dispose();
		}
	}
}
