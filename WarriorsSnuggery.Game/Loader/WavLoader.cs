using OpenTK.Audio.OpenAL;
using System.IO;
using WarriorsSnuggery.Audio;

namespace WarriorsSnuggery.Loader
{
	public static class WavLoader
	{
		public static unsafe void LoadWavFile(string path, out byte[] data, out int channels, out int sampleRate, out int bitDepth, out ALFormat format, out long musicSeekPosition)
		{
			using var reader = open(path, out channels, out sampleRate, out bitDepth, out var dataSize, out format, out musicSeekPosition);

			data = reader.ReadBytes(dataSize);
		}

		public static unsafe BinaryReader OpenWavFile(string path, out int channels, out int sampleRate, out int bitDepth, out int dataSize, out ALFormat format, out long musicSeekPosition)
		{
			return open(path, out channels, out sampleRate, out bitDepth, out dataSize, out format, out musicSeekPosition);
		}

		static unsafe BinaryReader open(string path, out int channels, out int sampleRate, out int bitDepth, out int dataSize, out ALFormat format, out long musicSeekPosition)
		{
			var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));

#pragma warning disable IDE0059
			int chunkID = reader.ReadInt32(); // Should be 'RIFF'
			int fileSize = reader.ReadInt32();
			int riffType = reader.ReadInt32(); // Should be 'WAVE'
			int fmtID = reader.ReadInt32(); // Should be 'fmt '
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

			//int dataID = reader.ReadInt32(); // Should be 'data'
			// Workaround: Skip all data until the 'data' keyword is found
			var wav = new byte[4];
			while (!(wav[0] == 100 && wav[1] == 97 && wav[2] == 116 && wav[3] == 97))
			{
				for (int i = 1; i < 4; i++)
					wav[i - 1] = wav[i];
				wav[3] = reader.ReadByte();
			}

			dataSize = reader.ReadInt32();
#pragma warning restore IDE0059

			if (channels == 1)
			{
				if (bitDepth == 8)
					format = ALFormat.Mono8;
				else if (bitDepth == 16)
					format = ALFormat.Mono16;
				else
					throw new InvalidSoundFileException($"Invalid .WAV file: Bitdepth is {bitDepth}, supported are 8 and 16.");
			}
			else if (channels == 2)
			{
				if (bitDepth == 8)
					format = ALFormat.Stereo8;
				else if (bitDepth == 16)
					format = ALFormat.Stereo16;
				else
					throw new InvalidSoundFileException($"Invalid .WAV file: Bitdepth is {bitDepth}, supported are 8 and 16.");
			}
			else
			{
				throw new InvalidSoundFileException($"Invalid .WAV file: Number of channels is {channels}, supported are mono and stereo.");
			}

			musicSeekPosition = reader.BaseStream.Position;

			return reader;
		}
	}
}
