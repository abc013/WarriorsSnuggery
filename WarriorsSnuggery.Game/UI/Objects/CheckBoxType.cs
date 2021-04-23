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
			Default = new BatchObject(@default);
			Checked = new BatchObject(@checked);
			Click = new BatchObject(click);
		}
	}
}
