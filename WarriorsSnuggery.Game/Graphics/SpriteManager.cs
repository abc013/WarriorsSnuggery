using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Graphics
{
	public static class SpriteManager
	{
		public static Sheet[] Sheets { get; private set; }
		public static int CurrentSheet { get; private set; }
		static bool sheetsLoaded;

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
				throw new OverflowException($"Tried to create new Sheet with index {CurrentSheet} (Max allowed: {Sheets.Length}). Try increasing the max sheet count.");

			Sheets[CurrentSheet] = new Sheet(Settings.SheetSize);
			SheetBuilder.UseSheet(Sheets[CurrentSheet]);
		}

		public static Texture[] AddTexture(TextureInfo info)
		{
			if (sheetsLoaded)
				throw new Exception($"Unable to add texture (file: {info.File}. Sheets are already loaded.");

			var hash = info.GetHashCode();

			if (hashedTextures.ContainsKey(hash))
				return hashedTextures[hash];

			Texture[] textures;
			if (info.Type == TextureType.IMAGE)
			{
				var data = BitmapLoader.LoadTexture(info.File, out var w, out var h);
				info = new TextureInfo(info.File, info.Type, 0, w, h, false);

				textures = new[] { addTexture(data, info) };
			}
			else
			{
				var dataList = BitmapLoader.LoadSprite(info.File, info.Width, info.Height);

				textures = new Texture[dataList.Count];
				for (int i = 0; i < dataList.Count; i++)
					textures[i] = addTexture(dataList[i], info);
			}

			hashedTextures.Add(hash, textures);

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
				var info = new TextureInfo(font.FontName, TextureType.IMAGE, 0, font.CharSizes[i], false);

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
			if (Settings.DeveloperMode)
			{
				int i = 0;
				foreach (var sheet in Sheets)
				{
					if (sheet == null)
						continue;

					BitmapSaver.Save($"{FileExplorer.Logs}spritesheet_{i}.png", TextureManager.GetContent(sheet.TextureID, sheet.Size.X, sheet.Size.Y), sheet.Size);
					i++;
				}
			}

			SheetBuilder.Clear();

			sheetsLoaded = true;
		}

		public static void DeleteTextures()
		{
			foreach (var sheet in Sheets)
				sheet?.Dispose();
		}
	}
}
