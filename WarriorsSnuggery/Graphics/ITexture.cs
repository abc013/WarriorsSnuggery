using OpenTK.Graphics.ES30;
using System;

namespace WarriorsSnuggery.Graphics
{
	public enum TextureType
	{
		IMAGE,
		ANIMATION,
		RANDOM
	}

	public sealed class TextureInfo
	{
		public readonly string File;

		public readonly TextureType Type;
		public readonly int Tick;

		public readonly int Width;
		public readonly int Height;

		public TextureInfo(string file, TextureType type, int tick, int width, int height, bool searchFile = true)
		{
			if (searchFile)
				File = FileExplorer.FindIn(FileExplorer.Misc, file);
			else
				File = file;

			Type = type;
			Tick = tick;

			Width = width;
			Height = height;
		}

		//if the type is random or image, it is certain that in this array there's only one texture.
		public ITexture[] GetTextures()
		{
			switch (Type)
			{
				case TextureType.RANDOM:
					var random = Program.SharedRandom;
					var images = SpriteManager.GetTexture(this);

					return new[] { images[random.Next(images.Length)] };
				default:
					return SpriteManager.GetTexture(this);
			}
		}

		public override int GetHashCode()
		{
			return File.GetHashCode() ^ Width ^ Height;
		}
	}

	public class ITexture : IDisposable
	{
		public readonly int SheetID;

		public readonly string File;

		public readonly MPos Offset;
		public readonly int Width;
		public readonly int Height;

		public ITexture(string file, int x, int y, int width, int height, int sheetID)
		{
			File = file;
			Offset = new MPos(x, y);
			Width = width;
			Height = height;
			SheetID = sheetID;
		}

		public ITexture(string file, int width, int height, int id)
		{
			File = file;
			Width = width;
			Height = height;
			SheetID = id;
		}

		public void Dispose()
		{
			lock (MasterRenderer.GLLock)
			{
				GL.DeleteTexture(SheetID);
				Program.CheckGraphicsError("Texture_Dispose");
			}
		}

		public override int GetHashCode()
		{
			return SheetID ^ Width ^ Height ^ File.GetHashCode();
		}
	}
}
