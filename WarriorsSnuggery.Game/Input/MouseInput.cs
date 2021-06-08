using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public static class MouseInput
	{
		static MouseState state => window.MouseState;
		static Window window;

		public static int WheelState { get; private set; }
		public static CPos WindowPosition { get; private set; }
		public static CPos GamePosition { get; private set; }
		static Vector vPos;

		public static bool IsLeftDown { get; private set; }
		public static bool IsMiddleDown { get; private set; }
		public static bool IsRightDown { get; private set; }

		static bool leftPressed, middlePressed, rightPressed;
		public static bool IsLeftClicked { get; private set; }
		public static bool IsMiddleClicked { get; private set; }
		public static bool IsRightClicked { get; private set; }

		public static void SetWindow(Window window)
		{
			MouseInput.window = window;
		}

		public static void Tick()
		{
			WheelState = 0;

			if (!WindowInfo.Focused)
			{
				IsLeftClicked = IsLeftDown = false;
				IsMiddleClicked = IsMiddleDown = false;
				IsRightClicked = IsRightDown = false;
				return;
			}

			IsLeftDown = state.IsButtonDown(MouseButton.Left);
			IsMiddleDown = state.IsButtonDown(MouseButton.Middle);
			IsRightDown = state.IsButtonDown(MouseButton.Right);
			IsLeftClicked = !IsLeftDown && leftPressed;
			IsMiddleClicked = !IsMiddleDown && middlePressed;
			IsRightClicked = !IsRightDown && rightPressed;

			leftPressed = IsLeftDown;
			middlePressed = IsMiddleDown;
			rightPressed = IsRightDown;
		}

		public static void UpdateMousePosition(float x, float y)
		{
			vPos = new Vector((x / WindowInfo.Width - 0.5f) * WindowInfo.Ratio, y / WindowInfo.Height - 0.5f, 0);

			WindowPosition = (vPos * new Vector(Camera.UIZoom, Camera.UIZoom, 1)).ToCPos();
			GamePosition = (vPos * new Vector(Camera.CurrentZoom, Camera.CurrentZoom, 1)).ToCPos() + Camera.LookAt;
		}

		public static void UpdateWheelState(int state)
		{
			WheelState = state;
		}

		public static void RecalculateMousePosition()
		{
			GamePosition = (vPos * new Vector(Camera.CurrentZoom, Camera.CurrentZoom, 1)).ToCPos() + Camera.LookAt;
		}

		public static float AngleToCursor(CPos pos)
		{
			var diff = WindowPosition - pos;

			return Angle.FromVector(diff.X, diff.Y);
		}

		public static CPos DistToCursor(CPos pos)
		{
			return WindowPosition - pos;
		}
	}
}
