using System;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Graphics
{
	public sealed class TextureInfo
	{
		readonly string filepath;
		readonly Texture[] textures;

		[Require, Desc("Name of the sprite file.")]
		public readonly PackageFile Name;
		[Require, Desc("Dimensions of the sprite.")]
		public readonly MPos Dimensions;

		[Desc("Use if you want to select a random texture out of the range.")]
		public readonly bool Randomized = false;
		[Desc("Sets the time it takes to change texture sprites.")]
		public readonly int Tick = 20;

		[Desc("Use if you want to customize the animation sequence.", "If no customization is done, indeces will be 0,1,2,...,n.")]
		public readonly int[] AnimationIndeces;
		[Desc("Reverses the animation sequence.")]
		public readonly bool ReverseAnimation;

		public int Width => Dimensions.X;
		public int Height => Dimensions.Y;

		public TextureInfo(TextNode node)
		{
			TypeLoader.SetValues(this, node.Children);
			filepath = FileExplorer.FindIn(Name.Package.ContentDirectory, Name.File, ".png");
			textures = SheetManager.AddSprite(filepath, Width, Height);

			if (AnimationIndeces != null)
			{
				var finalTextures = new Texture[AnimationIndeces.Length];
				for (int i = 0; i < AnimationIndeces.Length; i++)
					finalTextures[i] = textures[AnimationIndeces[i]];

				textures = finalTextures;
			}

			if (ReverseAnimation)
				Array.Reverse(textures);
		}

		public TextureInfo(PackageFile packageFile) : this(packageFile, new MPos(0, 0), load: false)
		{
			textures = SheetManager.AddTexture(filepath, out var width, out var height);
			Dimensions = new MPos(width, height);
		}

		public TextureInfo(PackageFile packageFile, MPos bounds, bool randomized = false, int tick = 20) : this(packageFile, bounds, randomized, tick, load: true) { }
		public TextureInfo(PackageFile packageFile, int width, int height, bool randomized = false, int tick = 20) : this(packageFile, new MPos(width, height), randomized, tick, load: true) { }

		TextureInfo(PackageFile packageFile, MPos bounds, bool randomized = false, int tick = 20, bool load = true)
		{
			Name = packageFile;
			filepath = FileExplorer.FindIn(Name.Package.ContentDirectory, Name.File, ".png");

			Randomized = randomized;
			Tick = tick;

			Dimensions = bounds;

			if (load)
				textures = SheetManager.AddSprite(filepath, Width, Height);
		}

		public Texture[] GetTextures()
		{
			if (textures == null)
				throw new System.Exception($"Tried to fetch textures from unloaded TextureInfo ({filepath}).");

			if (Randomized)
				return new[] { textures[Program.SharedRandom.Next(textures.Length)] };

			return textures;
		}
	}
}
