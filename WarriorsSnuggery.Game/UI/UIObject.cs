using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI
{
	public class UIObject : ITickRenderable
	{
		public virtual UIPos Position
		{
			get => position;
			set => position = value;
		}
		UIPos position;

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

		public virtual UIPos Bounds { get => bounds; set => bounds = value; }
		UIPos bounds;

		public UIPos SelectableBounds { get => selectableBounds; set => selectableBounds = value; }
		UIPos selectableBounds;

		protected bool ContainsMouse;

		public virtual void Tick() { }

		public virtual void Render() { }

		public virtual void DebugRender()
		{
			if (!Bounds.IsEmpty())
				ColorManager.DrawLineQuad(Position, Bounds, Color.Red);

			if (!SelectableBounds.IsEmpty())
				ColorManager.DrawLineQuad(Position, SelectableBounds, Color.Blue);
		}

		protected void CheckMouse()
		{
			var w = SelectableBounds.X;
			var h = SelectableBounds.Y;

			var mouse = MouseInput.WindowPosition;

			ContainsMouse = mouse.X > Position.X - w && mouse.X < Position.X + w && mouse.Y > Position.Y - h && mouse.Y < Position.Y + h;
		}

		public virtual void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt) { }
	}
}
