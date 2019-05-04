/*
 * User: Andreas
 * Date: 06.10.2018
 * Time: 20:03
 */
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class PanelList : Panel, IDisposable
	{
		public readonly MPos Size;
		readonly MPos intSize;
		readonly MPos itemSize;
		public MPos selected;
		public readonly List<PanelItem> Container = new List<PanelItem>();
		bool mouseOnPanel;
		int scrolled;

		public PanelList(CPos pos, MPos size, MPos itemSize, int bordersize, string texture, string border = "", string highlight = "") : base(pos, new MPos(size.X / 64 * 3, size.Y / 64 * 3), bordersize, texture, border, highlight != "" ? new ImageRenderable(TextureManager.Texture(highlight), new MPos(itemSize.X / 64 * 3, itemSize.Y / 64 * 3)) : null)
		{
			intSize = size;
			this.itemSize = itemSize;
			Size = new MPos((int) Math.Floor(size.X / itemSize.X + 0f), (int) Math.Floor(size.Y / itemSize.Y + 0f));
		}

		public void Add(PanelItem o)
		{
			Container.Add(o);
			var pos = getPosition(Container.Count - 1);
			o.Visible = pos.Y >= -intSize.Y && pos.Y <= intSize.Y;
			o.Position = Position + pos;
		}

		public void UpdatePositions()
		{
			for (int i = 0; i < Container.Count; i++)
			{
				var pos = getPosition(i);
				var o = Container[i];
				o.Visible = pos.Y >= -intSize.Y && pos.Y <= intSize.Y;
				o.Position = Position + pos;
			}
		}

		CPos getPosition(int pos)
		{
			var x = pos % Size.X;
			var y = pos / Size.X;

			var posX = -intSize.X + (x * 2 + 1) * itemSize.X;
			var posY = -intSize.Y + (y * 2 + 1) * itemSize.Y - scrolled * 2 * itemSize.Y;

			return new CPos(posX, posY, 0);
		}

		public override void Tick()
		{
			base.Tick();
			foreach(var o in Container)
				o.Tick();

			checkMouse();
			if (mouseOnPanel && Highlight != null)
			{
				var position = MouseInput.WindowPosition - Position + new CPos(intSize.X,intSize.Y,0);

				var x = (int) Math.Floor(position.X / (float) itemSize.X / 2);
				var y = (int) Math.Floor(position.Y / (float) itemSize.Y / 2);

				Highlight.SetPosition(Position + new CPos(-intSize.X + x * 2 * itemSize.X + itemSize.X, -intSize.Y + y * 2 * itemSize.Y + itemSize.Y, 0));
			}
			if (mouseOnPanel)
			{
				if ((scrolled < Math.Round(Container.Count / (float) Size.X - Size.Y) + 1) && (KeyInput.IsKeyDown("down", 5) || MouseInput.WheelState > 0))
				{
					scrolled++;
					UpdatePositions();
				}
				if (scrolled != 0 && (KeyInput.IsKeyDown("up", 5) || MouseInput.WheelState < 0))
				{
					scrolled--;
					UpdatePositions();
				}
			}
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;

			mouseOnPanel = mousePosition.X > Position.X - intSize.X && mousePosition.X < Position.X + intSize.X && mousePosition.Y > Position.Y - intSize.Y && mousePosition.Y < Position.Y + intSize.Y;
		}

		public override void Render()
		{
			HighlightVisible = mouseOnPanel;
			base.Render();

			foreach (var o in Container)
				o.Render();
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (var o in Container)
				o.Dispose();
		}
	}
}
