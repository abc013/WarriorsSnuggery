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
	}
}
