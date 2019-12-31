using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	public class SpriteManager
	{
		public static Sheet[] sheets;
		static int currentSheet;

		static readonly Dictionary<int, IImage[]> hashedTextures = new Dictionary<int, IImage[]>();

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
			var hash = info.GetHashCode();

			if (hashedTextures.ContainsKey(hash))
				return hashedTextures[hash];

			var data = TextureManager.loadSprite(info.File, info.Width, info.Height);
			var renderables = new IImage[data.Length];

			for (int i = 0; i < data.Length; i++)
			{
				if (!SheetBuilder.IsSpaceLeft(info.Width, info.Height))
					UseNextSheet();

				var texture = SheetBuilder.WriteTexture(data[i], info);
				var renderable = IImage.Create(Mesh.Image(texture, Color.White), texture);

				renderables[i] = renderable;
			}

			hashedTextures.Add(hash, renderables);

			return renderables;
		}

		public static IImage[] GetTexture(TextureInfo info)
		{
			return hashedTextures[info.GetHashCode()];
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
