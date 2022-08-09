using WarriorsSnuggery.Audio;

namespace WarriorsSnuggery.UI
{
	internal static class UIUtils
	{
		public const int TextMargin = 128;

		public static readonly string[] sellSounds = { "money_spent1", "money_spent2", "money_spent3" };

		public static void PlaySellSound()
		{
			var sellSound = sellSounds[Program.SharedRandom.Next(sellSounds.Length)];

			AudioManager.PlaySound(sellSound);
		}

		public static void PlayClickSound()
		{
			AudioManager.PlaySound("click");
		}

		public static void PlayPingSound()
		{
			AudioManager.PlaySound("ping");
		}

		public static void PlayLifeLostSound()
		{
			AudioManager.PlaySound("life_lost");
		}

		public static bool ContainsMouse(UIPositionable positionable)
		{
			var pos = positionable.Position;
			var bounds = positionable.SelectableBounds;

			var mouse = MouseInput.WindowPosition;

			return mouse.X > pos.X - bounds.X && mouse.X < pos.X + bounds.X && mouse.Y > pos.Y - bounds.Y && mouse.Y < pos.Y + bounds.Y;
		}
	}
}
