﻿using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class PanelListItem : UIObject, IDisableTooltip
	{
		public virtual bool Visible
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

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;
				renderable.SetRotation(value);
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;
				renderable.SetScale(value);
			}
		}

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

		public override void Render()
		{
			renderable.Render();
		}

		public override void Tick()
		{
			if (!Visible)
				return;

			CheckMouse();

			if (ContainsMouse)
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
