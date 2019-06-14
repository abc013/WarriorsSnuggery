using System;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class PanelItem : ITickRenderable, IDisposable
	{
		public virtual bool Visible
		{
			get { return renderable.Visible; }
			set { renderable.Visible = value; }
		}

		public virtual CPos Position
		{
			get { return position; }
			set
			{
				renderable.SetPosition(value);
				position = value;
			}
		}
		CPos position;

		public virtual VAngle Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;
				renderable.SetRotation(rotation);
			}
		}
		VAngle rotation;

		public virtual float Scale
		{
			get { return scale; }
			set
			{
				scale = value;
				renderable.SetScale(scale);
			}
		}
		float scale = 1f;

		readonly TextLine hoverText;
		readonly GraphicsObject renderable;
		readonly Action action;
		protected readonly MPos size;

		protected bool mouseOnItem;
		 
		public PanelItem(CPos pos, string hoverText, GraphicsObject renderable, MPos size, Action action)
		{
			this.hoverText = new TextLine(pos, IFont.Pixel16, TextLine.OffsetType.MIDDLE);
			this.hoverText.SetText(hoverText);
			this.hoverText.Visible = false;
			UIRenderer.RenderAfter(this.hoverText);
			this.renderable = renderable;
			this.action = action;
			this.size = size;
			position = pos;
		}

		public virtual void SetColor(Color color)
		{
			renderable.SetColor(color);
		}

		public virtual void Render()
		{
			renderable.Render();
		}

		public virtual void Tick()
		{
			if (!Visible)
				return;

			checkMouse();
			hoverText.Visible = mouseOnItem;
			if (mouseOnItem)
				hoverText.Position = MouseInput.WindowPosition;
		}

		public virtual void Dispose()
		{
			UIRenderer.RemoveRenderAfter(hoverText);
			hoverText.Dispose();
			//renderable.Dispose();
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;

			mouseOnItem = mousePosition.X > position.X - size.X && mousePosition.X < position.X + size.X && mousePosition.Y > position.Y - size.Y && mousePosition.Y < position.Y + size.Y;

			if (MouseInput.isLeftClicked && mouseOnItem)
				takeAction();
		}

		protected virtual void takeAction()
		{
			action?.Invoke();
		}
	}
}
