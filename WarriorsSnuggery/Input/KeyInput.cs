using OpenTK.Input;
using System;
using System.IO;

namespace WarriorsSnuggery
{
	public static class KeyInput
	{
		public readonly static char[] InvalidFileNameChars;

		public readonly static string[] AllKeys =
		{
			"↓",
			"↑",
			"←",
			"→",
			"-",
			"+",
			",",
			"q",
			"w",
			"e",
			"r",
			"t",
			"y",
			"u",
			"i",
			"o",
			"p",
			"a",
			"s",
			"d",
			"f",
			"g",
			"h",
			"j",
			"k",
			"l",
			"z",
			"x",
			"c",
			"v",
			"b",
			"n",
			"m"
		};

		static KeyboardState state;
		public static int HitCooldown;

		static KeyInput()
		{
			InvalidFileNameChars = Path.GetInvalidFileNameChars();
		}

		public static bool IsKeyDown(string key, int coolDownWhenHit = 0)
		{
			if (HitCooldown > 0 || !state.IsAnyKeyDown || !WindowInfo.Focused)
				return false;

			if (key.IndexOfAny(new[] { '↓', '↑', '←', '→', '-', '+', ',' }) == 0)
			{
				switch (key)
				{
					case "↓":
						key = "down";
						break;
					case "↑":
						key = "up";
						break;
					case "←":
						key = "left";
						break;
					case "→":
						key = "right";
						break;
					case "-":
						key = "minus";
						break;
					case "+":
						key = "plus";
						break;
					case ",":
						key = "comma";
						break;
				}
			}
			bool hit = state.IsKeyDown((Key)Enum.Parse(typeof(Key), key, true));

			if (hit)
				HitCooldown = coolDownWhenHit;

			return hit;
		}

		public static bool IsKeyDown(Key key, int coolDownWhenHit = 0)
		{
			if (HitCooldown > 0 || !state.IsAnyKeyDown || !WindowInfo.Focused)
				return false;

			bool hit = state.IsKeyDown(key);

			if (hit)
				HitCooldown = coolDownWhenHit;

			return hit;
		}

		public static void Tick()
		{
			HitCooldown--;
			state = Keyboard.GetState();
		}
	}
}
