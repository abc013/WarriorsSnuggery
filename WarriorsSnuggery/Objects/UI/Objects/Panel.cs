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

		public Panel(CPos position, MPos bounds, PanelType type) : this(position, bounds, type, null)
		{
			if (type.Background2 != null)
			{
				Highlight = new BatchObject(Mesh.UIPanel(type.Background2, Color.White, bounds), Color.White);
				Highlight.SetPosition(Position);
			}
		}

		public Panel(CPos position, MPos bounds, PanelType type, BatchObject background2)
		{
			background = new BatchObject(Mesh.UIPanel(type.Background, Color.White, bounds), Color.White);
			border = new BatchObject(Mesh.UIPanel(type.Border, Color.White, bounds + new MPos((int)(type.BorderWidth * 1024), (int)(type.BorderWidth * 1024))), Color.White);

			Highlight = background2;

			Bounds = new MPos(bounds.X + (int)(type.BorderWidth * 1024), bounds.Y + (int)(type.BorderWidth * 1024));
			SelectableBounds = bounds;

			Position = position;
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