using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class PanelList : Panel, IDisableTooltip, ITick
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				UpdatePositions();
			}
		}

		public readonly List<PanelListItem> Container = new List<PanelListItem>();

		public readonly UIPos Size;
		protected readonly UIPos ItemSize;

		readonly BatchObject highlightRenderable;
		protected (int x, int y) HighlightedPos = (-1, -1);
		public PanelListItem Highlighted => getItem(HighlightedPos.x, HighlightedPos.y);

		protected (int x, int y) SelectedPos = (-1, -1);
		public PanelListItem Selected => getItem(SelectedPos.x, SelectedPos.y);

		readonly bool autoHighlight;

		int currentScroll;

		public PanelList(UIPos size, UIPos itemSize, string type, bool autoHighlight = true) : this(size, itemSize, PanelCache.Types[type], autoHighlight) { }

		public PanelList(UIPos size, UIPos itemSize, PanelType type, bool autoHighlight = true) : base(size, type)
		{
			Size = new UIPos((int)Math.Floor(size.X / (float)itemSize.X), (int)Math.Floor(size.Y / (float)itemSize.Y));
			ItemSize = itemSize;

			this.autoHighlight = autoHighlight;
			highlightRenderable = type.Background2 == null ? null : new BatchObject(Mesh.UIPanel(type.Background2, itemSize));
		}

		public void Add(PanelListItem o)
		{
			Container.Add(o);
			var pos = getOffset(Container.Count - 1);
			o.Visible = pos.Y >= -SelectableBounds.Y && pos.Y <= SelectableBounds.Y;
			o.Position = Position + pos;
		}

		UIPos getOffset(int pos)
		{
			var x = pos % Size.X;
			var y = pos / Size.X;

			return getOffset(x, y);
		}

		UIPos getOffset(int x, int y)
		{
			var posX = (x * 2 + 1) * ItemSize.X - SelectableBounds.X;
			var posY = ((y - currentScroll) * 2 + 1) * ItemSize.Y - SelectableBounds.Y;

			return new UIPos(posX, posY);
		}

		PanelListItem getItem(int x, int y)
		{
			if (x < 0 || y < 0)
				return null;

			var offset = x + y * Size.X;

			if (offset >= Container.Count)
				return null;

			return Container[offset];
		}

		public virtual void DisableTooltip()
		{
			foreach (var o in Container)
				o.DisableTooltip();
		}

		public virtual void Tick()
		{
			foreach (var o in Container)
				o.Tick();

			if (UIUtils.ContainsMouse(this))
			{
				if (autoHighlight)
				{
					var offset = MouseInput.WindowPosition - Position;

					var itemOffsetX = (int)Math.Floor((offset.X + SelectableBounds.X) / (float)(ItemSize.X * 2));
					var itemOffsetY = (int)Math.Floor((offset.Y + SelectableBounds.Y) / (float)(ItemSize.Y * 2)) + currentScroll;

					HighlightedPos = (itemOffsetX, itemOffsetY);

					if (Highlighted == null)
						HighlightedPos = (-1, -1);

					if (MouseInput.IsLeftClicked)
					{
						UIUtils.PlayClickSound();
						SelectedPos = HighlightedPos;
					}
				}

				if ((currentScroll < Math.Floor(Container.Count / (float)Size.X - Size.Y) + 1) && (KeyInput.IsKeyDown(Keys.Down) || MouseInput.WheelState > 0))
				{
					currentScroll++;
					UpdatePositions();
				}
				if (currentScroll != 0 && (KeyInput.IsKeyDown(Keys.Up) || MouseInput.WheelState < 0))
				{
					currentScroll--;
					UpdatePositions();
				}
			}
			else if (autoHighlight)
				HighlightedPos = (-1, -1);
		}

		protected void UpdatePositions()
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
			base.Render();

			if (HighlightedPos.x >= 0 && HighlightedPos.y >= 0)
			{
				highlightRenderable.SetPosition(Position + getOffset(HighlightedPos.x, HighlightedPos.y));
				highlightRenderable.Render();
			}

			foreach (var o in Container)
				o.Render();

			if (SelectedPos.x >= 0 && SelectedPos.y >= 0)
			{
				var pos = getOffset(SelectedPos.x, SelectedPos.y);

				if (pos.Y >= -SelectableBounds.Y && pos.Y <= SelectableBounds.Y)
				{
					pos += Position;

					var radius = (int)(MathF.Sin(Window.GlobalTick / 32f) * 32) + 64;
					ColorManager.DrawGlowingFilledLineRect(pos - ItemSize, pos + ItemSize, 16, Color.White, radius, 8);
				}
			}
		}
	}
}
