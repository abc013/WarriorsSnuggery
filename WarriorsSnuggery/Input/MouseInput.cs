using OpenTK.Input;
using System;

namespace WarriorsSnuggery
{
	public static class MouseInput
	{
		public static int WheelState;
		static int wheelValue;
		public static CPos WindowPosition;
		public static CPos GamePosition;
		static Vector vPos;

		public static bool IsLeftDown { get; private set; }
		public static bool IsMiddleDown { get; private set; }
		public static bool IsRightDown { get; private set; }

		static bool leftPressed, middlePressed, rightPressed;
		public static bool IsLeftClicked { get; private set; }
		public static bool IsMiddleClicked { get; private set; }
		public static bool IsRightClicked { get; private set; }

		public static void Tick()
		{
			if (!WindowInfo.Focused)
			{
				WheelState = 0;
				IsLeftClicked = IsLeftDown = false;
				IsMiddleClicked = IsMiddleDown = false;
				IsRightClicked = IsRightDown = false;
				return;
			}

			var change = Mouse.GetCursorState().Wheel;
			WheelState = wheelValue - change;
			wheelValue = change;

			var state = Mouse.GetState();
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

		public static void UpdateMousePosition(MPos pos)
		{
			var x = (pos.X - WindowInfo.Width / 2f) / WindowInfo.Width * WindowInfo.Ratio;
			var y = (pos.Y - WindowInfo.Height / 2f) / WindowInfo.Height;
			vPos = new Vector(x, y, 0);

			WindowPosition = (vPos * new Vector(Camera.DefaultZoom, Camera.DefaultZoom, 1)).ToCPos();
			GamePosition = (vPos * new Vector(Camera.CurrentZoom, Camera.CurrentZoom, 1)).ToCPos() + Camera.LookAt;
		}

		public static void RecalculateMousePosition()
		{
			GamePosition = (vPos * new Vector(Camera.CurrentZoom, Camera.CurrentZoom, 1)).ToCPos() + Camera.LookAt;
		}

		public static float AngleToCursor(CPos pos)
		{
			var diff = WindowPosition - pos;

			float angle = (float)-Math.Atan2(diff.Y, diff.X);
			float degrees = (180 / (float)Math.PI) * angle;
			degrees = degrees > 0 ? degrees : degrees + 360f;
			return degrees;
		}

		public static CPos DistToCursor(CPos pos)
		{
			return WindowPosition - pos;
		}
	}
}
