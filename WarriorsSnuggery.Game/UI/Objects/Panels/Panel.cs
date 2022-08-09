using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class Panel : UIPositionable, IRenderable
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				background.SetPosition(value);
				border.SetPosition(value);
				highlight?.SetPosition(value);
			}
		}

		public virtual Color Color
		{
			get => color;
			set
			{
				color = value;
				background.SetColor(value);
				border.SetColor(value);
				highlight?.SetColor(value);
			}
		}
		Color color;

		readonly BatchObject background;
		readonly BatchObject border;
		readonly BatchObject highlight;

		public bool HighlightVisible;

		public Panel(UIPos bounds, string typeName, bool useHighlight = false) : this(bounds, PanelCache.Types[typeName], useHighlight) { }

		public Panel(UIPos bounds, PanelType type, bool useHighlight = false)
		{
			SelectableBounds = bounds;
			background = new BatchObject(Mesh.UIPanel(type.Background, bounds));

			Bounds = bounds + new UIPos(type.BorderWidth, type.BorderWidth);
			border = new BatchObject(Mesh.UIPanel(type.Border, Bounds));

			if (useHighlight && type.Background2 != null)
				highlight = new BatchObject(Mesh.UIPanel(type.Background2, bounds));
		}

		public virtual void Render()
		{
			border.Render();
			background.Render();

			if (HighlightVisible && highlight != null)
				highlight.Render();
		}
	}
}