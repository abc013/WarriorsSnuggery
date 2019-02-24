using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class TextRenderable : GraphicsObject
	{
		public const float SizeMultiplier = GLPos.PixelMultiplier / 4;

		public TextRenderable(CPos position, IFont font, char @char, Color color, int curTextWidth = 0) :
			base(new IChar(font, @char, color, font.Mesh))
		{
			setPosition(position.ToVector() + new OpenTK.Vector4(curTextWidth * SizeMultiplier, 0,0,0));
		}

		public override void setColor(Color color)
		{
			((IChar) renderable).SetColor(color);
		}

		public void setChar(char @char)
		{
			((IChar) renderable).SetChar(@char);
		}
	}

	public class FrameRenderable : GraphicsObject
	{
		public FrameRenderable(ITexture texture) :
			base(IImage.Create(TexturedMesh.Frame(), texture))
		{ }
	}

	public class ImageRenderable : GraphicsObject
	{
		public ImageRenderable(ITexture texture, float scale = 1f) :
			base(IImage.Create(TexturedMesh.Plane(scale * (texture.Width > texture.Height ? texture.Width/24f : texture.Height/24f), texture.Width, texture.Height), texture))
		{ }

		public ImageRenderable(ITexture texture, MPos size, float scale = 1f) :
			base(IImage.Create(TexturedMesh.PixelOrientedPlane(scale, size.X, size.Y), texture))
		{ }
	}

	public class SpriteRenderable : GraphicsObject
	{
		readonly int tick;
		int curTick;
		int curTexture;

		public SpriteRenderable(ITexture[] textures, float scale = 1f, int tick = 10) :
			base(ISprite.Create(TexturedMesh.Plane(scale * (textures[0].Width > textures[0].Height ? textures[0].Width/24f : textures[0].Height/24f), textures[0].Width, textures[0].Height), textures, tick))
		{
			this.tick = tick;
			curTick = tick;
		}

		public override void Render()
		{
			if (curTick-- <= 0)
			{
				curTick = tick;
				curTexture++;
				if (curTexture >= ((ISprite)renderable).textures.Length)
					curTexture = 0;
			}
			((ISprite)renderable).CurTexture = curTexture;
			base.Render();
		}
	}

	public class TerrainRenderable : GraphicsObject
	{
		public TerrainRenderable(TerrainType type) :
			base(IImage.Create(TexturedMesh.Terrain(), type.Texture)) { }
	}

	public class WallRenderable : GraphicsObject
	{
		public WallRenderable(bool horizontal, WallType type) :
			base(IImage.Create(TexturedMesh.Plane(2f, 24, 48), type.GetTexture(horizontal))) { }
	}
}
