using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class PanelType
	{
		public readonly Texture Background;
		public readonly Texture Background2;
		public readonly Texture Border;
		public readonly float BorderWidth;

		public PanelType(Texture background, Texture background2, Texture border, float borderWidth)
		{
			Background = background;
			Background2 = background2;
			Border = border;
			BorderWidth = borderWidth;
		}
	}
}
