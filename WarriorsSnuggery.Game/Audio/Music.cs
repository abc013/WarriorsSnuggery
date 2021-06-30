using OpenTK.Audio.OpenAL;
using System.IO;

namespace WarriorsSnuggery.Audio
{
	public class Music
	{
		public readonly int Length;
		int currentLength;
		bool paused;

		readonly BinaryReader reader;

		readonly int bufferSize;

		public bool Done => currentLength <= 0;

		readonly ALFormat format;
		readonly int sampleRate;

		readonly MusicAudioBufferRotator rotator;

		MusicAudioSource source;
		bool firstTick = true;

		public Music(string path)
		{
			reader = Loader.WavLoader.OpenWavFile(path, out int channels, out sampleRate, out int bitDepth, out int dataSize, out format);

			bufferSize = sampleRate * channels * bitDepth/8;

			Length = AudioUtils.GetLengthInTicks(dataSize, channels, sampleRate, bitDepth);

			rotator = new MusicAudioBufferRotator(dataSize, bufferSize, channels, sampleRate, bitDepth);

			// Fill first 3 buffers
			fillBuffer();
			fillBuffer();
			fillBuffer();
		}

		public void Play()
		{
			currentLength = Length;
			source = AudioController.MusicSource;
			source.SetVolume(Settings.MusicVolume, Settings.MasterVolume);

			// Queue first 2 buffers
			source.QueueBuffer(nextBuffer());
			source.QueueBuffer(nextBuffer());
		}

		public void UpdateVolume()
		{
			source?.UpdateVolume(Settings.MusicVolume * Settings.MasterVolume);
		}

		public void Tick()
		{
			if (firstTick)
				source.Start();

			firstTick = false;

			if (!paused)
			{
				currentLength--;
				if (!Done)
				{
					// Let the rotation begin!
					var buffers = source.BuffersProcessed();
					for (int i = 0; i < buffers; i++)
					{
						// Fill next buffer
						if (rotator.CurrentWriteRotation < rotator.MaxRotations)
							fillBuffer();

						source.UnqueueBuffer();
						// Unqueue last buffer and queue next one
						if (rotator.CurrentReadRotation < rotator.MaxRotations)
							source.QueueBuffer(nextBuffer());
					}

					// If something in tick took too long, the source stops playing automatically. Recognize stop and restart playing.
					if (buffers >= MusicAudioBufferRotator.BufferCount - 1)
						source.Start();
				}
			}
		}

		void fillBuffer()
		{
			var dataArray = reader.ReadBytes(bufferSize);
			rotator.WriteAndRotate(dataArray, format, sampleRate);
		}

		MusicAudioBuffer nextBuffer()
		{
			return rotator.ReadAndRotate();
		}

		public void Pause(bool pause)
		{
			paused = pause;
			source?.Pause(pause);
		}

		public void Stop()
		{
			source?.Stop();
			source = null;
		}

		public void Dispose()
		{
			rotator.Dispose();
			reader.Dispose();
		}
	}
}
