using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class UITextBlock : UIObject
	{
		readonly TextBlock block;

		public TextLine[] Lines => block.Lines;

		public override CPos Position
		{
			get => block.Position;
			set => block.Position = value;
		}

		public override float Scale
		{
			get => block.Scale;
			set => block.Scale = value;
		}

		public override VAngle Rotation
		{
			get => block.Rotation;
			set => block.Rotation = value;
		}

		public UITextBlock(CPos pos, Font font, TextOffset offset, params string[] text)
		{
			block = new TextBlock(pos, font, offset, text);
		}

		public override void Tick()
		{
			block.Tick();
		}

		public override void Render()
		{
			block.Render();
		}

		public override void DebugRender()
		{
			// TODO
		}
	}
}
