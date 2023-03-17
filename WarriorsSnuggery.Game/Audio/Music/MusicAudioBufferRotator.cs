using OpenTK.Audio.OpenAL;
using System;

namespace WarriorsSnuggery.Audio.Music
{
	internal class MusicAudioBufferRotator
	{
		public const int BufferCount = 3;

		public readonly int MaxRotations;
		public int CurrentWriteRotation { get; private set; }
		public int CurrentReadRotation { get; private set; }

		readonly MusicAudioBuffer[] buffers;

		public MusicAudioBufferRotator(int totalSize, int bufferSize, int channels, int sampleRate, int bitDepth)
		{
			MaxRotations = (int)Math.Ceiling(totalSize / (float)bufferSize);

			buffers = new MusicAudioBuffer[BufferCount];
			for (int i = 0; i < BufferCount; i++)
				buffers[i] = new MusicAudioBuffer();
		}

		public void WriteAndRotate(byte[] data, ALFormat format, int sampleRate)
		{
			if (CurrentWriteRotation >= MaxRotations)
				throw new Exception($"Attempted to write on rotate buffer more than allowed ({MaxRotations})");

			buffers[CurrentWriteRotation++ % BufferCount].LoadBuffer(data, format, sampleRate);
		}

		public MusicAudioBuffer ReadAndRotate()
		{
			if (CurrentReadRotation >= MaxRotations)
				throw new Exception($"Attempted to read on rotate buffer more than allowed ({MaxRotations})");

			return buffers[CurrentReadRotation++ % BufferCount];
		}

		public void Reset()
		{
			CurrentReadRotation = 0;
			CurrentWriteRotation = 0;
		}

		public void Dispose()
		{
			for (int i = 0; i < BufferCount; i++)
				buffers[i].Dispose();
		}
	}
}
