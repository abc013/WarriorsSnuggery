using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Graphics
{
	public class TextRenderable : GraphicsObject
	{
		public const float SizeMultiplier = VectorConvert.PixelMultiplier / 4;
		static readonly Vector shadowVector = new Vector(-0.04f / WindowInfo.Ratio, -0.04f, 0f);
		public Color Color;
		public char @Char;

		public TextRenderable(CPos position, IFont font, char @char, Color color, int curTextWidth = 0) : base(font == IFont.Pixel16 ? CharManager.Pixel16 : CharManager.Papyrus24)
		{
			SetPosition(position.ToVector() + new Vector(curTextWidth * SizeMultiplier, 0, 0, 0));
			Color = color;
			@Char = @char;
		}

		void setColor(Color color)
		{
			((IChar)renderable).SetColor(color);
		}

		void setChar(char @char)
		{
			((IChar)renderable).SetChar(@char);
		}

		public override void Render()
		{
			setChar(@Char);
			if (Settings.EnableTextShadowing && Color != Color.Black)
			{
				setColor(Color.Black);
				SetPosition((Vector)position + shadowVector);
				base.Render();
				SetPosition((Vector)position - shadowVector);
			}
			setColor(Color);
			base.Render();
		}

		public override void Dispose()
		{
			Visible = false;
		}
	}

	public class FrameRenderable : GraphicsObject
	{
		public FrameRenderable(ITexture texture) : base(IImage.Create(TexturedMesh.Frame(), texture)) { }
	}

	public class ImageRenderable : GraphicsObject
	{
		public ImageRenderable(ITexture texture, float scale = 1f) : base(IImage.Create(TexturedMesh.Plane(scale * (texture.Width > texture.Height ? texture.Width / 24f : texture.Height / 24f), texture.Width, texture.Height), texture)) { }

		public ImageRenderable(ITexture texture, MPos size, float scale = 1f) : base(IImage.Create(TexturedMesh.PixelOrientedPlane(scale, size.X, size.Y), texture)) { }
	}

	public class SpriteRenderable : GraphicsObject
	{
		readonly int tick;
		int curTick;
		int curTexture;

		public SpriteRenderable(ITexture[] textures, float scale = 1f, int tick = 10) : base(ISprite.Create(TexturedMesh.Plane(scale * (textures[0].Width > textures[0].Height ? textures[0].Width / 24f : textures[0].Height / 24f), textures[0].Width, textures[0].Height), textures, tick))
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
		public TerrainRenderable(TerrainType type) : base(IImage.Create(TexturedMesh.Terrain(), type.Texture)) { }
	}

	public class WallRenderable : GraphicsObject
	{
		public WallRenderable(bool horizontal, WallType type) : base(IImage.Create(TexturedMesh.Plane(2f, 24, 48), type.GetTexture(horizontal))) { }
	}
}
