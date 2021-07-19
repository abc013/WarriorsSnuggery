using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	public class Panel : UIObject
	{
		public override CPos Position
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

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;
				background.SetRotation(value);
				border.SetRotation(value);
				highlight?.SetRotation(value);
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;
				background.SetScale(value);
				border.SetScale(value);
				highlight?.SetScale(value);
			}
		}

		public override Color Color
		{
			get => base.Color;
			set
			{
				base.Color = value;
				background.SetColor(value);
				border.SetColor(value);
				highlight?.SetColor(value);
			}
		}

		readonly BatchObject background;
		readonly BatchObject border;
		readonly BatchObject highlight;

		public bool HighlightVisible;

		public Panel(MPos bounds, string typeName, bool useHighlight = false) : this(bounds, PanelCache.Types[typeName], useHighlight) { }

		public Panel(MPos bounds, PanelType type, bool useHighlight = false)
		{
			SelectableBounds = bounds;
			background = new BatchObject(Mesh.UIPanel(type.Background, bounds));

			Bounds = new MPos(bounds.X + type.BorderWidth, bounds.Y + type.BorderWidth);
			border = new BatchObject(Mesh.UIPanel(type.Border, Bounds));

			if (useHighlight && type.Background2 != null)
				highlight = new BatchObject(Mesh.UIPanel(type.Background2, bounds));
		}

		public override void Render()
		{
			border.Render();
			background.Render();

			if (HighlightVisible && highlight != null)
				highlight.Render();
		}
	}
}