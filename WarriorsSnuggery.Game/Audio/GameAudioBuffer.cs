﻿using OpenTK.Audio.OpenAL;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Audio
{
	public class GameAudioBuffer : AudioBuffer
	{
		public override int BufferID => bufferID;
		readonly int bufferID;

		public GameAudioBuffer(string path)
		{
			bufferID = AL.GenBuffer();

			WavLoader.LoadWavFile(path, out byte[] data, out _, out int sampleRate, out _, out var format);

			LoadData(data, format, sampleRate);
		}

		public override void Dispose()
		{
			AL.DeleteBuffer(bufferID);
		}
	}
}
