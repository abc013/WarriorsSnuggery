using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class PanelList : Panel, IDisableTooltip
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				updatePositions();
			}
		}

		public readonly List<PanelListItem> Container = new List<PanelListItem>();
		public readonly MPos Size;

		protected readonly MPos ItemSize;

		(int x, int y) highlighted = (-1, -1);
		public PanelListItem Highlighted => getItem(highlighted.x, highlighted.y);

		(int x, int y) selected = (-1, -1);
		public PanelListItem Selected => getItem(selected.x, selected.y);

		int scrolled;

		public PanelList(MPos size, MPos itemSize, string type) : this(size, itemSize, PanelManager.Get(type)) { }

		public PanelList(MPos size, MPos itemSize, PanelType type) : base(size, type, type.Background2 != null ? new BatchObject(Mesh.UIPanel(type.Background2, itemSize)) : null)
		{
			ItemSize = itemSize;
			Size = new MPos((int)Math.Floor(size.X / (float)itemSize.X), (int)Math.Floor(size.Y / (float)itemSize.Y));
		}

		public void Add(PanelListItem o)
		{
			Container.Add(o);
			var pos = getOffset(Container.Count - 1);
			o.Visible = pos.Y >= -SelectableBounds.Y && pos.Y <= SelectableBounds.Y;
			o.Position = Position + pos;
		}

		CPos getOffset(int pos)
		{
			var x = pos % Size.X;
			var y = pos / Size.X;

			return getOffset(x, y);
		}

		CPos getOffset(int x, int y)
		{
			var posX = (x * 2 + 1) * ItemSize.X - SelectableBounds.X;
			var posY = (y * 2 + 1) * ItemSize.Y - SelectableBounds.Y - scrolled * 2 * ItemSize.Y;

			return new CPos(posX, posY, 0);
		}

		PanelListItem getItem(int x, int y)
		{
			if (x < 0 || y < 0)
				return null;

			var offset = x * Size.Y + y;

			if (offset >= Container.Count)
				return null;

			return Container[offset];
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

			CheckMouse();
			if (ContainsMouse)
			{
				var offset = MouseInput.WindowPosition - Position;

				var itemOffsetX = (int)Math.Floor((offset.X + SelectableBounds.X) / (float)ItemSize.X / 2);
				var itemOffsetY = (int)Math.Floor((offset.Y + SelectableBounds.Y) / (float)ItemSize.Y / 2);

				highlighted = (itemOffsetX, itemOffsetY);
				if (MouseInput.IsLeftClicked)
					selected = highlighted;

				Highlight?.SetPosition(Position + getOffset(highlighted.x, highlighted.y));

				if ((scrolled < Math.Floor(Container.Count / (float)Size.X - Size.Y) + 1) && (KeyInput.IsKeyDown(Keys.Down) || MouseInput.WheelState > 0))
				{
					scrolled++;
					updatePositions();
				}
				if (scrolled != 0 && (KeyInput.IsKeyDown(Keys.Up) || MouseInput.WheelState < 0))
				{
					scrolled--;
					updatePositions();
				}
			}
			else
			{
				highlighted = (-1, -1);
				//if (MouseInput.IsLeftClicked)
				//	selected = (-1, -1);
			}
		}

		void updatePositions()
		{
			for (int i = 0; i < Container.Count; i++)
			{
				var pos = getOffset(i);
				var o = Container[i];
				o.Visible = pos.Y >= -SelectableBounds.Y && pos.Y <= SelectableBounds.Y;
				o.Position = Position + pos;
			}
		}

		public override void Render()
		{
			HighlightVisible = ContainsMouse;
			base.Render();

			foreach (var o in Container)
				o.Render();

			if (selected.x >= 0 && selected.y >= 0)
			{
				var pos = Position + getOffset(selected.x, selected.y);
				ColorManager.DrawFilledLineRect(pos - new CPos(ItemSize.X, ItemSize.Y, 0), pos + new CPos(ItemSize.X, ItemSize.Y, 0), 32, Color.White);
			}
		}
	}
}
