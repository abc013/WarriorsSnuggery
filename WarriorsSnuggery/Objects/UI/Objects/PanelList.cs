using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class PanelList : Panel, IDisableTooltip
	{
		public readonly List<PanelItem> Container = new List<PanelItem>();
		public readonly MPos Size;

		protected readonly MPos intSize;
		protected readonly MPos itemSize;

		public MPos selected;
		int scrolled;

		public PanelList(CPos pos, MPos size, MPos itemSize, PanelType type) : base(pos, new Vector(size.X / 1024f, size.Y / 1024f, 0), type, type.Background2 != null ? new BatchObject(Mesh.UIPlane(type.Background2, Color.White, new Vector(itemSize.X / 1024f, itemSize.Y / 1024f, 0)), Color.White) : null)
		{
			intSize = size;
			this.itemSize = itemSize;
			Size = new MPos((int)Math.Floor(size.X / (float)itemSize.X), (int)Math.Floor(size.Y / (float)itemSize.Y));
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

		public virtual void DisableTooltip()
		{
			foreach (var o in Container)
				o.DisableTooltip();
		}

		public override void Tick()
		{
			base.Tick();
			foreach (var o in Container)
				o.Tick();

			CheckMouse(intSize.X, intSize.Y);
			if (ContainsMouse)
			{
				if (Highlight != null)
				{
					var position = MouseInput.WindowPosition - Position + new CPos(intSize.X, intSize.Y, 0);

					var x = (int)Math.Floor(position.X / (float)itemSize.X / 2);
					var y = (int)Math.Floor(position.Y / (float)itemSize.Y / 2);

					Highlight.SetPosition(Position + new CPos(-intSize.X + x * 2 * itemSize.X + itemSize.X, -intSize.Y + y * 2 * itemSize.Y + itemSize.Y, 0));
				}

				if ((scrolled < Math.Floor(Container.Count / (float)Size.X - Size.Y) + 1) && (KeyInput.IsKeyDown("down", 5) || MouseInput.WheelState > 0))
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

		public override void Render()
		{
			HighlightVisible = ContainsMouse;
			base.Render();

			foreach (var o in Container)
				o.Render();
		}
	}
}
