using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class CheckBoxType
	{
		public readonly BatchObject Default;
		public readonly BatchObject Checked;
		public readonly BatchObject Click;

		public CheckBoxType(Texture @default, Texture @checked, Texture click)
		{
			Default = new BatchObject(@default, Color.White);
			Checked = new BatchObject(@checked, Color.White);
			Click = new BatchObject(click, Color.White);
		}
	}
}
