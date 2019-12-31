using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Graphics
{
	public class TextRenderable : GraphicsObject
	{
		static readonly Vector shadowVector = new Vector(-0.04f / WindowInfo.Ratio, -0.04f, 0f);
		public Color Color;
		public char @Char;

		public TextRenderable(CPos position, IFont font, char @char, Color color, int curTextWidth = 0) : base(font == IFont.Pixel16 ? CharManager.Pixel16 : CharManager.Papyrus24)
		{
			SetPosition(position.ToVector() + new Vector(curTextWidth * IFont.FontSizeMultiplier, 0, 0, 0));
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
		public FrameRenderable(ITexture texture) : base(IImage.Create(Mesh.Frame(), texture)) { }
	}

	public class ImageRenderable : GraphicsObject
	{
		public ImageRenderable(IImage image) : base(image) { }

		public ImageRenderable(ITexture texture, float scale = 1f) : base(IImage.Create(Mesh.Plane(scale * (texture.Width > texture.Height ? texture.Width * MasterRenderer.PixelMultiplier : texture.Height * MasterRenderer.PixelMultiplier), texture.Width, texture.Height), texture)) { }

		public ImageRenderable(ITexture texture, MPos size, float scale = 1f) : base(IImage.Create(Mesh.PixelOrientedPlane(scale, size.X, size.Y), texture)) { }
	}

	public class IImageSequenceRenderable : GraphicsObject
	{
		readonly IImage[] images;
		readonly int tick;
		readonly bool pauseable;
		int curTick;
		int curImage;

		// Create empty GraphicsObject
		public IImageSequenceRenderable(IImage[] images, int tick, bool pauseable = false) : base(null)
		{
			this.images = images;
			this.tick = tick;
			this.pauseable = pauseable;
			curTick = tick;
		}

		public override void Render()
		{
			if (!(pauseable && MasterRenderer.PauseSequences) && curTick-- <= 0)
			{
				curTick = tick;
				curImage++;
				if (curImage >= images.Length)
					curImage = 0;
			}
			renderable = images[curImage];
			base.Render();
		}
	}
}
