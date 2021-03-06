﻿using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class TextPanel : Panel
	{
		readonly List<UITextLine> lines = new List<UITextLine>();

		readonly Font font;

		readonly int lineHeight;
		readonly int lineCount;
		int lineScroll;

		CPos textPosition => Position - new CPos(SelectableBounds.X - font.Width, -SelectableBounds.Y + lineHeight, 0);

		public TextPanel(CPos position, MPos bounds, Font font, PanelType type) : base(position, bounds, type)
		{
			this.font = font;
			lineHeight = font.Height;
			lineCount = bounds.Y / lineHeight;
		}

		public void Add(string message, bool timeStamp = true)
		{
			if (timeStamp)
				message = DateTime.Now.ToString("[HH:mm] ") + message;

			var line = new UITextLine(CPos.Zero, font);
			line.WriteText(message);
			lines.Add(line);

			moveLines();
		}

		public override void Render()
		{
			base.Render();

			for (int i = lines.Count - lineScroll; i > lines.Count - lineScroll - lineCount; i--)
			{
				if (i <= 0)
					break;

				lines[i - 1].Render();
			}
			// Render the last ten lines
		}

		void moveLines()
		{
			for(int i = lines.Count; i > 0; i--)
				lines[^i].Position = textPosition + new CPos(0, -((i - 1) - lineScroll) * lineHeight * 2, 0);
		}

		public override void Tick()
		{
			CheckMouse();

			if (ContainsMouse && MouseInput.WheelState != 0)
			{
				lineScroll -= MouseInput.WheelState;

				var maximum = Math.Max(lines.Count - lineCount, 0);
				lineScroll = Math.Clamp(lineScroll, 0, maximum);
				moveLines();
			}
		}
	}
}
