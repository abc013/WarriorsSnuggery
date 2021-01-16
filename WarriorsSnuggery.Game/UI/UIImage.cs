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

		readonly BatchObject @object;

		public UIImage(CPos pos, BatchObject @object, float scale = 1f)
		{
			this.@object = @object;
			@object.SetPosition(pos);
			@object.SetScale(scale);
		}

		public override void Render()
		{
			@object.PushToBatchRenderer();
		}
	}
}
