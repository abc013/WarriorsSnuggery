using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Audio
{
	public static class SoundController
	{
		static readonly Dictionary<string, SoundAudioBuffer> buffers = new Dictionary<string, SoundAudioBuffer>();

		public static SoundAudioSource PlaySound(string packageFile)
		{
			return AudioController.Play(buffers[packageFile], false, 1f, 1f, Vector.Zero, false);
		}

		public static SoundAudioSource PlaySound(string packageFile, bool inGame, float volume, float pitch, Vector position, bool loops = false)
		{
			return AudioController.Play(buffers[packageFile], inGame, volume, pitch, position, loops);
		}

		public static SoundAudioBuffer LoadSound(PackageFile packageFile)
		{
			var key = packageFile.ToString();
			if (buffers.ContainsKey(key))
				return buffers[key];

			var filePath = FileExplorer.FindIn(packageFile.Package.ContentDirectory, packageFile.File, ".wav");
			var buffer = new SoundAudioBuffer(filePath);

			buffers.Add(key, buffer);

			return buffer;
		}

		internal static void Dispose()
		{
			foreach (var buffer in buffers.Values)
				buffer.Dispose();

			buffers.Clear();
		}
	}
}
