using System;

namespace WarriorsSnuggery.Audio
{
	public class SoundInfo
	{
		[Desc("Describes whether the sound has to be player ingame.")]
		public readonly bool InGame;

		[Desc("Loops the sound until the specific length is played.")]
		public readonly bool Loops;

		[Desc("Length to play in seconds.")]
		public readonly float Length;

		[Desc("Basic volume in percent.")]
		public readonly float Volume;

		[Desc("Name of the audio file.")]
		public readonly string Name;

		public readonly AudioBuffer Buffer;

		public SoundInfo(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			Buffer = AudioManager.GetBuffer(Name);
		}
	}

	public class Sound
	{
		readonly SoundInfo info;
		public CPos Position;

		AudioSource source;
		int length;

		public Sound(SoundInfo info)
		{
			this.info = info;
		}

		public void Play()
		{
			source = AudioController.Play(info.Buffer, info.InGame, info.Volume, info.Loops);
			length = (int)Math.Ceiling(info.Length * 30);
		}

		public void Tick()
		{
			length--;
			if (length-- <= 0 && info.Length > 0)
			{
				Stop();
			}
		}

		public void Pause(bool pause)
		{
			if (source == null)
				return;

			source.Pause(pause);
		}

		public void Stop()
		{
			if (source == null)
				return;

			source.Stop();
			source = null;
		}
	}
}
