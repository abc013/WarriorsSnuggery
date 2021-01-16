namespace WarriorsSnuggery.Graphics
{
	public class TextRenderable : IRenderable
	{
		BatchObject charRenderable;
		static readonly Vector shadowOffset = new Vector(0.02f * WindowInfo.Ratio, -0.02f, 0);
		Vector position;

		readonly Font font;
		Color color;

		public TextRenderable(CPos position, Font font, char c, Color color, int pxOffset = 0)
		{
			this.font = font;
			charRenderable = new BatchObject(Mesh.Character(font, c), color);
			SetPosition(position, pxOffset);
			SetColor(color);
		}

		public void SetPosition(CPos position, int pxOffset = 0)
		{
			SetPosition(position.ToVector(), pxOffset);
		}

		public void SetPosition(Vector position, int pxOffset = 0)
		{
			this.position = position + new Vector(pxOffset / 512f, 2 * font.PixelMultiplier, 0);
			charRenderable.SetPosition(this.position);
		}

		public void SetScale(float scale)
		{
			charRenderable.SetScale(scale);
		}

		public void SetScale(Vector scale)
		{
			charRenderable.SetScale(scale);
		}

		public void SetRotation(VAngle rotation)
		{
			charRenderable.SetRotation(rotation);
		}

		public void SetColor(Color color)
		{
			this.color = color;
			charRenderable.SetColor(color);
		}

		public void SetCharacter(char c)
		{
			charRenderable = new BatchObject(Mesh.Character(font, c), color);
		}

		public void Render()
		{
			if (Settings.EnableTextShadowing)
			{
				charRenderable.SetColor(Color.Black);
				charRenderable.SetPosition(position + shadowOffset);
				charRenderable.PushToBatchRenderer();
				charRenderable.SetPosition(position - shadowOffset);
				charRenderable.SetColor(color);
			}
			charRenderable.PushToBatchRenderer();
		}
	}
}
