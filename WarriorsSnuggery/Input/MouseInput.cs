using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public static class MouseInput
	{
		public static MouseState State;
		public static int WheelState;
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

			IsLeftDown = State.IsButtonDown(MouseButton.Left);
			IsMiddleDown = State.IsButtonDown(MouseButton.Middle);
			IsRightDown = State.IsButtonDown(MouseButton.Right);
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
