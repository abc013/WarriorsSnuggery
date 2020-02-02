using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class CheckBoxType
	{
		public readonly BatchObject Default;
		public readonly BatchObject Checked;
		public readonly BatchObject Click;

		public readonly float Height;
		public readonly float Width;

		public CheckBoxType(ITexture @default, ITexture @checked, ITexture click, float height, float width)
		{
			Default = new BatchObject(@default, Color.White);
			Default.SetScale(1.5f);
			Checked = new BatchObject(@checked, Color.White);
			Checked.SetScale(1.5f);
			Click = new BatchObject(click, Color.White);
			Click.SetScale(1.5f);

			Height = height * 512;
			Width = width * 512;
		}
	}
}
