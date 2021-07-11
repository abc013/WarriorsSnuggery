using OpenTK.Audio.OpenAL;

namespace WarriorsSnuggery.Audio
{
	public class MusicAudioBuffer : AudioBuffer
	{
		public override int BufferID => bufferID;
		int bufferID;

		public MusicAudioBuffer() { }

		public void LoadBuffer(byte[] data, ALFormat format, int sampleRate)
		{
			bufferID = AL.GenBuffer();

			LoadData(data, format, sampleRate);
		}

		public override void Dispose()
		{
			AL.DeleteBuffer(bufferID);
		}
	}
}
