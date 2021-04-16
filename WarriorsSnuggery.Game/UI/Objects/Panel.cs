using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
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
				Highlight?.SetPosition(value);
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
				Highlight?.SetRotation(value);
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
				Highlight?.SetScale(value);
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
				Highlight?.SetColor(value);
			}
		}

		readonly BatchObject background;
		readonly BatchObject border;
		public readonly BatchObject Highlight;

		public bool HighlightVisible;

		public Panel(MPos bounds, string typeName) : this(bounds, PanelManager.Get(typeName)) { }

		public Panel(MPos bounds, PanelType type) : this(bounds, type, null)
		{
			if (type.Background2 != null)
				Highlight = new BatchObject(Mesh.UIPanel(type.Background2, Color.White, bounds), Color.White);
		}

		public Panel(MPos bounds, PanelType type, BatchObject background2)
		{
			SelectableBounds = bounds;
			background = new BatchObject(Mesh.UIPanel(type.Background, Color.White, bounds), Color.White);

			Bounds = new MPos(bounds.X + type.BorderWidth, bounds.Y + type.BorderWidth);
			border = new BatchObject(Mesh.UIPanel(type.Border, Color.White, Bounds), Color.White);

			Highlight = background2;
		}

		public override void Render()
		{
			border.PushToBatchRenderer();
			background.PushToBatchRenderer();

			if (HighlightVisible && Highlight != null)
				Highlight.PushToBatchRenderer();
		}
	}
}