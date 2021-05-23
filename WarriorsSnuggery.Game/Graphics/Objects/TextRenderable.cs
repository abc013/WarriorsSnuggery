namespace WarriorsSnuggery.Graphics
{
	public class TextRenderable : IRenderable
	{
		BatchObject charRenderable;
		static readonly Vector shadowOffset = new Vector(0.02f * WindowInfo.Ratio, -0.02f, 0);
		Vector position;

		readonly Font font;
		Color color;

		public TextRenderable(Font font, char c, Color color)
		{
			this.font = font;

			SetCharacter(c);
			SetColor(color);
		}

		public void SetPosition(CPos pos, int offset = 0)
		{
			position = pos.ToVector() + new Vector(offset / 1024f, 0, 0);
			charRenderable.SetPosition(position);
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
			charRenderable = new BatchObject(Mesh.Character(font, c));
			charRenderable.SetColor(color);
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
