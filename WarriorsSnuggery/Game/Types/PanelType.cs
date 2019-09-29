/*
 * User: Andreas
 * Date: 07.07.2018
 * Time: 00:21
 */

namespace WarriorsSnuggery.UI
{
	public class PanelType
	{
		public readonly string Background;
		public readonly string Background2;
		public readonly string Border;
		public readonly int BorderWidth;

		public PanelType(string background, string background2, string border, int borderWidth)
		{
			Background = background;
			Background2 = background2;
			Border = border;
			BorderWidth = borderWidth;
		}
	}
}
