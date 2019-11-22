using OpenTK.Audio.OpenAL;
using System;
using System.Runtime.InteropServices;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Audio
{
	public class AudioBuffer
	{
		uint buffer;
		public readonly int Length;
		readonly float seconds;

		public AudioBuffer(string path)
		{
			AL.GenBuffer(out this.buffer);

			ALFormat format;
			WavLoader.LoadWavFile(path, out byte[] buffer, out int channels, out int sampleRate, out int bitDepth);

			seconds = buffer.Length / (sampleRate * channels * bitDepth / 8f);
			Length = (int)Math.Ceiling(seconds * 60);
			if (channels == 1)
			{
				if (bitDepth == 8)
					format = ALFormat.Mono8;
				else if (bitDepth == 16)
					format = ALFormat.Mono16;
				else
					throw new InvalidSoundFileException(string.Format("Invalid .WAV file: Bitdepth is {0}, supported are 8 and 16.", bitDepth));
			}
			else if (channels == 2)
			{
				if (bitDepth == 8)
					format = ALFormat.Stereo8;
				else if (bitDepth == 16)
					format = ALFormat.Stereo16;
				else
					throw new InvalidSoundFileException(string.Format("Invalid .WAV file: Bitdepth is {0}, supported are 8 and 16.", bitDepth));
			}
			else
			{
				throw new InvalidSoundFileException(string.Format("Invalid .WAV file: Number of channels is {0}, supportet are mono and stereo.", channels));
			}

			unsafe
			{
				GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
				IntPtr pointer = pinnedArray.AddrOfPinnedObject();

				AL.BufferData(this.buffer, format, pointer, buffer.Length, sampleRate);

				pinnedArray.Free();
			}
		}

		public uint GetID()
		{
			return buffer;
		}

		public void Dispose()
		{
			AL.DeleteBuffer(ref buffer);
		}
	}
}
