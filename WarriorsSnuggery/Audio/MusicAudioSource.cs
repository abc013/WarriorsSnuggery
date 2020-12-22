using OpenTK.Audio.OpenAL;

namespace WarriorsSnuggery.Audio
{
	public class MusicAudioSource : AudioSource
	{
		public new void Start()
		{
			base.Start();
		}

		public void QueueBuffer(AudioBuffer buffer)
		{
			AL.SourceQueueBuffer(Source, buffer.BufferID);
		}

		public void UnqueueBuffer()
		{
			var id = AL.SourceUnqueueBuffer(Source);

			// Delete buffer instantly. It won't be used anymore
			AL.DeleteBuffer(id);
		}

		public int BuffersProcessed()
		{
			AL.GetSource(Source, ALGetSourcei.BuffersProcessed, out var count);

			return count;
		}

		protected override void ResetData()
		{
			base.ResetData();

			AL.GetSource(Source, ALGetSourcei.BuffersQueued, out int buffers);
			if (buffers > 0)
				AL.SourceUnqueueBuffers(Source, buffers);
		}
	}
}
