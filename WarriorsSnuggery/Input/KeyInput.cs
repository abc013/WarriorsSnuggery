using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.IO;

namespace WarriorsSnuggery
{
	public static class KeyInput
	{
		public readonly static char[] InvalidFileNameChars;

		public static KeyboardState State;
		public static int HitCooldown;

		static KeyInput()
		{
			InvalidFileNameChars = Path.GetInvalidFileNameChars();
		}

		public static Keys ToKey(string key)
		{
			return (Keys)Enum.Parse(typeof(Keys), key, true);
		}

		public static bool IsKeyDown(Keys key, int coolDownWhenHit = 0)
		{
			if (HitCooldown > 0 || !State.IsAnyKeyDown || !WindowInfo.Focused)
				return false;

			bool hit = State.IsKeyDown(key);

			if (hit)
				HitCooldown = coolDownWhenHit;

			return hit;
		}

		public static void Tick()
		{
			HitCooldown--;
		}
	}
}
