/*
 * User: Andreas
 * Date: 12.10.2017
 * 
 */
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
				case TextureType.ANIMATION:
					return TextureManager.Sprite(this);
				case TextureType.RANDOM:
					return new[] { TextureManager.RandomTexture(this) };
				default:
					return new[] { TextureManager.Texture(File) };
			}
		}
	}

	public class ITexture : IDisposable
	{
		public readonly int ID;

		public readonly string File;

		public readonly int Width;
		public readonly int Height;

		public ITexture(string file, int width, int height, int id)
		{
			File = file;
			Width = width;
			Height = height;
			ID = id;
		}

		public void Dispose()
		{
			lock (MasterRenderer.GLLock)
			{
				GL.DeleteTexture(ID);
				Program.CheckGraphicsError("Texture_Dispose");
			}
		}

		public override int GetHashCode()
		{
			return ID ^ Width ^ Height ^ File.GetHashCode();
		}
	}
}
