using OpenTK.Mathematics;

namespace WarriorsSnuggery.Graphics
{
	public class CharRenderable : BatchRenderable
	{
		static readonly Vector3 shadowOffset = new Vector3(0.04f * WindowInfo.Ratio, -0.04f, 0);

		public CharRenderable(Font font, char c, Color color) : base(Mesh.Character(font, c))
		{
			SetColor(color);
		}

		public void SetPosition(Vector3 pos, int widthOffset = 0, int heightOffset = 0)
		{
			var position = pos + new Vector3(widthOffset / 1024f, heightOffset / 1024f, 0);
			base.SetPosition(position);
		}

		public override void Render()
		{
			if (Settings.EnableTextShadowing)
			{
				var color = Color;
				var position = Position;

				SetColor(Color.Black);
				SetPosition(position + shadowOffset);
				base.Render();
				SetPosition(position);
				SetColor(color);
			}
			base.Render();
		}
	}
}
