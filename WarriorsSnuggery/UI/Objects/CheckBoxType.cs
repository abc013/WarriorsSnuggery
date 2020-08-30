using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class CheckBoxType
	{
		public readonly BatchObject Default;
		public readonly BatchObject Checked;
		public readonly BatchObject Click;

		public readonly int Height;
		public readonly int Width;

		public CheckBoxType(Texture @default, Texture @checked, Texture click, float height, float width)
		{
			Default = new BatchObject(@default, Color.White);
			Default.SetScale(1.5f);
			Checked = new BatchObject(@checked, Color.White);
			Checked.SetScale(1.5f);
			Click = new BatchObject(click, Color.White);
			Click.SetScale(1.5f);

			Height = (int)(height * 512);
			Width = (int)(width * 512);
		}
	}
}
