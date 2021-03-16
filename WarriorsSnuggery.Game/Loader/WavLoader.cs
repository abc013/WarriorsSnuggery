using OpenTK.Audio.OpenAL;
using System.IO;
using WarriorsSnuggery.Audio;

namespace WarriorsSnuggery.Loader
{
	public static class WavLoader
	{
		public static unsafe void LoadWavFile(string path, out byte[] data, out int channels, out int sampleRate, out int bitDepth, out ALFormat format)
		{
			using var reader = open(path, out channels, out sampleRate, out bitDepth, out var dataSize, out format);

			data = reader.ReadBytes(dataSize);
		}

		public static unsafe BinaryReader OpenWavFile(string path, out int channels, out int sampleRate, out int bitDepth, out int dataSize, out ALFormat format)
		{
			return open(path, out channels, out sampleRate, out bitDepth, out dataSize, out format);
		}

		static unsafe BinaryReader open(string path, out int channels, out int sampleRate, out int bitDepth, out int dataSize, out ALFormat format)
		{
			var reader = new BinaryReader(new FileStream(path, FileMode.Open));

#pragma warning disable IDE0059
			int chunkID = reader.ReadInt32();
			int fileSize = reader.ReadInt32();
			int riffType = reader.ReadInt32();
			int fmtID = reader.ReadInt32();
			int fmtSize = reader.ReadInt32();
			int fmtCode = reader.ReadInt16();
			channels = reader.ReadInt16();
			sampleRate = reader.ReadInt32();
			int fmtAvgBPS = reader.ReadInt32();
			int fmtBlockAlign = reader.ReadInt16();
			bitDepth = reader.ReadInt16();

			if (fmtSize == 18)
			{
				// Read any extra values
				int fmtExtraSize = reader.ReadInt16();
				reader.ReadBytes(fmtExtraSize);
			}

			int dataID = reader.ReadInt32();
			dataSize = reader.ReadInt32();
#pragma warning restore IDE0059

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
				throw new InvalidSoundFileException(string.Format("Invalid .WAV file: Number of channels is {0}, supported are mono and stereo.", channels));
			}

			return reader;
		}
	}
}
