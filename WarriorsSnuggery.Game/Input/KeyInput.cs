using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.IO;

namespace WarriorsSnuggery
{
	public static class KeyInput
	{
		static KeyboardState state => window.KeyboardState;
		static Window window;

		public readonly static char[] InvalidFileNameChars;

		public static string Text;

		static KeyInput()
		{
			InvalidFileNameChars = Path.GetInvalidFileNameChars();
		}

		public static void SetWindow(Window window)
		{
			KeyInput.window = window;
		}

		public static void Tick()
		{
			Text = string.Empty;
		}

		public static Keys ToKey(string key)
		{
			return (Keys)Enum.Parse(typeof(Keys), key, true);
		}

		public static bool IsKeyDown(Keys key)
		{
			if (state == null)
				return false;

			if (!state.IsAnyKeyDown || !WindowInfo.Focused)
				return false;

			bool hit = state.IsKeyDown(key);

			return hit;
		}
	}
}
