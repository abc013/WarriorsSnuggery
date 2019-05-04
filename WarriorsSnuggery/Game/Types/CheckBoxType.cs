/*
 * User: Andreas
 * Date: 17.09.2018
 * Time: 15:58
 */
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class CheckBoxType
	{
		public readonly GraphicsObject Default;
		public readonly GraphicsObject Checked;
		public readonly GraphicsObject Click;

		public readonly float Height;
		public readonly float Width;

		public CheckBoxType(GraphicsObject @default, GraphicsObject @checked, GraphicsObject click, float height, float width)
		{
			Default = @default;
			Checked = @checked;
			Click = click;

			Height = height * 512;
			Width = width * 512;
		}
	}
}
