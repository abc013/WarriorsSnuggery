namespace WarriorsSnuggery.UI
{
	public class UIObject : IPositionable, ITickRenderable
	{
		public virtual CPos Position
		{
			get => position;
			set => position = value;
		}
		CPos position;

		public virtual VAngle Rotation
		{
			get => rotation;
			set => rotation = value;
		}
		VAngle rotation;

		public virtual float Scale
		{
			get => scale;
			set => scale = value;
		}
		float scale = 1f;

		public virtual Color Color
		{
			get => color;
			set => color = value;
		}
		Color color = Color.White;

		protected bool ContainsMouse;

		public virtual void Tick() { }

		public virtual void Render() { }

		protected void CheckMouse(int width, int height)
		{
			var mousePosition = MouseInput.WindowPosition;

			ContainsMouse = mousePosition.X > Position.X - width && mousePosition.X < Position.X + width && mousePosition.Y > Position.Y - height && mousePosition.Y < Position.Y + height;
		}
	}
}
