/*
 * User: Andreas
 * Date: 07.07.2018
 * Time: 00:21
 */

namespace WarriorsSnuggery.Objects
{
	public class PanelType
	{
		public readonly string DefaultString;
		public readonly string ActiveString;
		public readonly string BorderString;
		public readonly int Border;

		public readonly int Height;
		public readonly int Width;

		public PanelType(float height, float width, string defaultString, string activeString, string borderString, int border)
		{
			Height = (int) (height * 512);
			Width = (int) (width * 512);

			DefaultString = defaultString;
			BorderString = borderString;
			Border = border;
			ActiveString = activeString;
		}
	}
}
