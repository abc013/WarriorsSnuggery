using OpenTK.Audio.OpenAL;

namespace WarriorsSnuggery.Audio
{
	public class GameAudioSource : AudioSource
	{
		public void Start(AudioBuffer buffer, bool loops)
		{
			AL.BindBufferToSource(Source, buffer.BufferID);
			AL.Source(Source, ALSourceb.Looping, loops);

			Start();
		}
	}
}
