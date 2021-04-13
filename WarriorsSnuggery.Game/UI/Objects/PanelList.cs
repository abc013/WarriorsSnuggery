using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
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

		public readonly List<PanelItem> Container = new List<PanelItem>();
		public readonly MPos Size;

		protected readonly MPos itemSize;

		public MPos selected;
		int scrolled;

		public PanelList(CPos pos, MPos size, MPos itemSize, PanelType type) : base(pos, size, type, type.Background2 != null ? new BatchObject(Mesh.UIPanel(type.Background2, Color.White, itemSize), Color.White) : null)
		{
			this.itemSize = itemSize;
			Size = new MPos((int)Math.Floor(size.X / (float)itemSize.X), (int)Math.Floor(size.Y / (float)itemSize.Y));
		}

		public void Add(PanelItem o)
		{
			Container.Add(o);
			var pos = getPosition(Container.Count - 1);
			o.Visible = pos.Y >= -SelectableBounds.Y && pos.Y <= SelectableBounds.Y;
			o.Position = Position + pos;
		}

		CPos getPosition(int pos)
		{
			var x = pos % Size.X;
			var y = pos / Size.X;

			var posX = -SelectableBounds.X + (x * 2 + 1) * itemSize.X;
			var posY = -SelectableBounds.Y + (y * 2 + 1) * itemSize.Y - scrolled * 2 * itemSize.Y;

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

			CheckMouse();
			if (ContainsMouse)
			{
				if (Highlight != null)
				{
					var position = MouseInput.WindowPosition - Position + new CPos(SelectableBounds.X, SelectableBounds.Y, 0);

					var x = (int)Math.Floor(position.X / (float)itemSize.X / 2);
					var y = (int)Math.Floor(position.Y / (float)itemSize.Y / 2);

					Highlight.SetPosition(Position + new CPos(-SelectableBounds.X + x * 2 * itemSize.X + itemSize.X, -SelectableBounds.Y + y * 2 * itemSize.Y + itemSize.Y, 0));
				}

				if ((scrolled < Math.Floor(Container.Count / (float)Size.X - Size.Y) + 1) && (KeyInput.IsKeyDown(Keys.Down, 5) || MouseInput.WheelState > 0))
				{
					scrolled++;
					updatePositions();
				}
				if (scrolled != 0 && (KeyInput.IsKeyDown(Keys.Up, 5) || MouseInput.WheelState < 0))
				{
					scrolled--;
					updatePositions();
				}
			}
		}

		void updatePositions()
		{
			for (int i = 0; i < Container.Count; i++)
			{
				var pos = getPosition(i);
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
		}
	}
}
