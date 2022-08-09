using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class UIImage : UIPositionable, IRenderable
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				@object.SetPosition(value);
			}
		}

		public virtual VAngle Rotation
		{
			get => rotation;
			set
			{
				rotation = value;
				@object.SetRotation(value);
			}
		}
		VAngle rotation;

		public virtual float Scale
		{
			get => scale;
			set
			{
				scale = value;
				@object.SetScale(value);
			}
		}
		float scale;

		public virtual Color Color
		{
			get => color;
			set
			{
				color = value;
				@object.SetColor(value);
			}
		}
		Color color;

		readonly BatchObject @object;

		public UIImage(BatchObject @object)
		{
			this.@object = @object;
		}

		public void Render()
		{
			@object.Render();
		}
	}
}
