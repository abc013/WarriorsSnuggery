using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class UIImage : UIObject
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				@object.SetPosition(value);
			}
		}

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;
				@object.SetRotation(value);
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;
				@object.SetScale(value);
			}
		}

		public override Color Color
		{
			get => base.Color;
			set
			{
				base.Color = value;
				@object.SetColor(value);
			}
		}

		readonly BatchObject @object;

		public UIImage(BatchObject @object)
		{
			this.@object = @object;
		}

		public override void Render()
		{
			@object.PushToBatchRenderer();
		}
	}
}
