using OpenTK.Input;
using System;

namespace WarriorsSnuggery
{
	public static class MouseInput
	{
		public static int WheelState;
		static int WheelValue;
		public static CPos WindowPosition;
		public static CPos GamePosition;

		public static bool isLeftDown { get; private set; }
		public static bool isMiddleDown { get; private set; }
		public static bool isRightDown { get; private set; }

		static bool lefwaspres, midwaspres, rigwaspres;
		public static bool isLeftClicked { get; private set; }
		public static bool isMiddleClicked { get; private set; }
		public static bool isRightClicked { get; private set; }

		public static void Tick()
		{
			var change = Mouse.GetCursorState().Wheel;
			WheelState = WheelValue - change;
			WheelValue = change;

			var state = Mouse.GetState();
			isLeftDown = state.IsButtonDown(MouseButton.Left);
			isMiddleDown = state.IsButtonDown(MouseButton.Middle);
			isRightDown = state.IsButtonDown(MouseButton.Right);
			isLeftClicked = !isLeftDown && lefwaspres;
			isMiddleClicked = !isMiddleDown && midwaspres;
			isRightClicked = !isRightDown && rigwaspres;

			GamePosition = VectorConvert.ToCPos(Window.ExactMousePosition * new Vector(Camera.CurrentZoom, Camera.CurrentZoom, 1, 1)) + Camera.LookAt;

			lefwaspres = isLeftDown;
			midwaspres = isMiddleDown;
			rigwaspres = isRightDown;
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
