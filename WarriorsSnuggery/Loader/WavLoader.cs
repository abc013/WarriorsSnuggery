using System.IO;

namespace WarriorsSnuggery.Loader
{
	public static class WavLoader
	{
		public static unsafe void LoadWavFile(string path, out byte[] data, out int channels, out int sampleRate, out int bitDepth)
		{
			using (var reader = new BinaryReader(new FileStream(path, FileMode.Open)))
			{
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
				int dataSize = reader.ReadInt32();
				data = reader.ReadBytes(dataSize);
			}
		}

		static double bytesToDouble(byte firstByte, byte secondByte)
		{
			// convert two bytes to one short (little endian)
			int s = (secondByte << 8) | firstByte;
			// convert to range from -1 to (just below) 1
			return s / 32768.0;
		}
	}
}
