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

		public void SetPosition(Vector3 position)
		{
			base.SetPosition(position);
		}

		public override void Render()
		{
			if (Settings.EnableTextShadowing)
			{
				var color = Color;
				var position = Position;

				SetColor(new Color(0, 0, 0, color.A));
				SetPosition(position + Vector3.ComponentMin(Scale, Vector3.One) * shadowOffset);
				base.Render();
				SetPosition(position);
				SetColor(color);
			}
			base.Render();
		}
	}
}
