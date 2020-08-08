using WarriorsSnuggery.Graphics;

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

		public virtual MPos Bounds { get; protected set; }

		protected MPos SelectableBounds;

		protected bool ContainsMouse;

		public virtual void Tick() { }

		public virtual void Render() { }

		public virtual void DebugRender()
		{
			if (Bounds != MPos.Zero)
				ColorManager.DrawLineRect(Position, new CPos(Bounds.X, Bounds.Y, 0), Color.Red);

			if (SelectableBounds != MPos.Zero)
				ColorManager.DrawLineRect(Position, new CPos(SelectableBounds.X, SelectableBounds.Y, 0), Color.Blue);
		}

		protected void CheckMouse()
		{
			var w = SelectableBounds.X;
			var h = SelectableBounds.Y;

			var mouse = MouseInput.WindowPosition;

			ContainsMouse = mouse.X > Position.X - w && mouse.X < Position.X + w && mouse.Y > Position.Y - h && mouse.Y < Position.Y + h;
		}
	}
}
