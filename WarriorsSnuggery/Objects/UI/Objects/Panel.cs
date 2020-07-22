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

		public virtual MPos Bounds { get; private set; }

		readonly BatchObject background;
		readonly BatchObject border;
		public readonly BatchObject Highlight;

		public bool HighlightVisible;

		public Panel(CPos position, Vector bounds, PanelType type) : this(position, bounds, type, type.Background2 != null ? new BatchObject(Mesh.UIPlane(type.Background2, Color.White, bounds), Color.White) : null) { }

		public Panel(CPos position, Vector bounds, PanelType type, BatchObject background2)
		{
			background = new BatchObject(Mesh.UIPlane(type.Background, Color.White, bounds), Color.White);
			border = new BatchObject(Mesh.UIPlane(type.Border, Color.White, bounds + new Vector(type.BorderWidth, type.BorderWidth, 0)), Color.White);

			if (background2 != null)
				Highlight = background2;

			Bounds = new MPos((int)((bounds.X + type.BorderWidth) * MasterRenderer.PixelMultiplier * 512), (int)((bounds.Y + type.BorderWidth) * MasterRenderer.PixelMultiplier * 512));

			Position = position;
		}

		public override void Render()
		{
			border.PushToBatchRenderer();
			background.PushToBatchRenderer();

			if (HighlightVisible && Highlight != null)
				Highlight.PushToBatchRenderer();
		}

		public override void Tick() { }
	}
}