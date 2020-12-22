using System;

namespace WarriorsSnuggery.Audio
{
	public static class AudioUtils
	{
		public static float GetLengthInSeconds(int size, int channels, int sampleRate, int bitDepth)
		{
			return size / (sampleRate * channels * bitDepth / 8f);
		}

		public static int GetLengthInTicks(int size, int channels, int sampleRate, int bitDepth)
		{
			var seconds = GetLengthInSeconds(size, channels, sampleRate, bitDepth);
			return (int)Math.Ceiling(seconds * Settings.UpdatesPerSecond);
		}
	}
}
