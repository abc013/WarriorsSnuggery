using OpenToolkit.Windowing.Common.Input;
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

		public static bool IsKeyDown(string key, int coolDownWhenHit = 0)
		{
			if (HitCooldown > 0 || !State.IsAnyKeyDown || !WindowInfo.Focused)
				return false;

			bool hit = State.IsKeyDown((Key)Enum.Parse(typeof(Key), key, true));

			if (hit)
				HitCooldown = coolDownWhenHit;

			return hit;
		}

		public static bool IsKeyDown(Key key, int coolDownWhenHit = 0)
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
