using System;
using System.Linq;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public class Tooltip : IRenderable, IDisposable
	{
		const int margin = 96;

		public CPos Position
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
				setPosition();
			}
		}
		CPos position;

		readonly TextLine title;
		readonly TextBlock text;

		readonly CPos size;

		public Tooltip(CPos pos, string title, params string[] text)
		{
			this.title = new TextLine(CPos.Zero, IFont.Pixel16);
			this.title.WriteText(title);
			this.text = new TextBlock(CPos.Zero, IFont.Pixel16, TextLine.OffsetType.LEFT, text);
			Position = pos;

			var xChars = this.title.String.Length;
			if (text.Length != 0)
			{
				var maxInText = this.text.Lines.Max(s => s.String.Length);

				if (maxInText > xChars)
					xChars = maxInText;
			}

			size = new CPos(xChars * 256 + 2 * margin, text.Length * (512 + margin) + 512 + 3 * margin, 0);
		}

		public void Render()
		{
			Position = MouseInput.WindowPosition + new CPos(256, 0, 0);
			if (Position.X + size.X > WindowInfo.UnitWidth * 512)
				Position -= new CPos(size.X, 0, 0);

			ColorManager.DrawRect(position, position + size, new Color(0, 0, 0, 0.8f));
			ColorManager.LineWidth = 3f;
			ColorManager.DrawLine(position, position + new CPos(size.X, 0, 0), Color.White);
			ColorManager.DrawLine(position, position + new CPos(0, size.Y, 0), Color.White);
			ColorManager.DrawLine(position + new CPos(size.X, 0, 0), position + new CPos(size.X, size.Y, 0), Color.White);
			ColorManager.DrawLine(position + new CPos(0, size.Y, 0), position + new CPos(size.X, size.Y, 0), Color.White);
			ColorManager.ResetLineWidth();
			title.Render();
			text.Render();
		}

		void setPosition()
		{
			title.Position = position + new CPos(480 + margin, 320 + margin, 0);
			text.Position = position + new CPos(480 + margin, 896 + margin, 0);
		}

		public void Dispose()
		{
			title.Dispose();
			text.Dispose();
		}
	}
}
