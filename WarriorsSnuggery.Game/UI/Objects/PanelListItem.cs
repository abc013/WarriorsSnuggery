using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class PanelListItem : UIPositionable, IDisableTooltip, ITick, IRenderable
	{
		public bool Visible
		{
			get => renderable.Visible;
			set => renderable.Visible = value;
		}

		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				renderable.SetPosition(value);
			}
		}

		public float Scale
		{
			get => scale;
			set
			{
				scale = value;
				renderable.SetScale(value);
			}
		}
		float scale;

		readonly BatchRenderable renderable;
		readonly Action action;
		readonly Tooltip tooltip;

		public PanelListItem(BatchRenderable renderable, UIPos bounds, string title, string[] text, Action action)
		{
			tooltip = new Tooltip(title, text);
			this.renderable = renderable;
			this.action = action;

			Bounds = bounds;
			SelectableBounds = bounds;
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

			if (UIUtils.ContainsMouse(this))
			{
				UIRenderer.SetTooltip(tooltip);

				if (MouseInput.IsLeftClicked)
					takeAction();
			}
			else
				UIRenderer.DisableTooltip(tooltip);
		}

		public virtual void DisableTooltip()
		{
			UIRenderer.DisableTooltip(tooltip);
		}

		protected virtual void takeAction()
		{
			action?.Invoke();
		}
	}
}
