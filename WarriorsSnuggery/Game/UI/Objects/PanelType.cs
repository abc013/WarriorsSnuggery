using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class PanelType
	{
		public readonly ITexture Background;
		public readonly ITexture Background2;
		public readonly ITexture Border;
		public readonly float BorderWidth;

		public PanelType(ITexture background, ITexture background2, ITexture border, float borderWidth)
		{
			Background = background;
			Background2 = background2;
			Border = border;
			BorderWidth = borderWidth;
		}
	}
}
