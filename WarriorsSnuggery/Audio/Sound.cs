using WarriorsSnuggery.Audio;

namespace WarriorsSnuggery
{
	[Desc("Needed to play sound on specific events.")]
	public class SoundType
	{
		[Desc("Basic volume in percent.")]
		public readonly float Volume = 1f;

		[Desc("Maximum random volume in percent.")]
		public readonly float RandomVolume = 0f;

		[Desc("Pitch in percent from 0.5 to 1.5.")]
		public readonly float Pitch = 1f;

		[Desc("Maximum random pitch in percent.")]
		public readonly float RandomPitch = 0f;

		[Desc("Name of the audio file.")]
		public readonly string Name;

		public readonly AudioBuffer Buffer;

		public SoundType(MiniTextNode[] nodes, bool isDocumentation = false)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (!isDocumentation)
			{
				AudioManager.LoadSound(Name, FileExplorer.FindPath(FileExplorer.Misc, Name, ".wav"));
				Buffer = AudioManager.GetBuffer(Name);
			}
		}
	}

	public class Sound
	{
		const float reduction = 1000000f;

		readonly SoundType info;
		readonly bool inGame;
		readonly float defaultVolume;
		readonly float defaultPitch;
		AudioSource source;
		float dist;

		public Sound(SoundType info, bool inGame = true)
		{
			this.info = info;
			this.inGame = inGame;
			defaultVolume = info.Volume + info.RandomVolume * (float)(Program.SharedRandom.NextDouble() - 0.5);
			defaultPitch = info.Pitch + info.RandomPitch * (float)(Program.SharedRandom.NextDouble() - 0.5);
		}

		public void Play(CPos position, bool loops)
		{
			var vector = convert(position);
			dist = vector.Dist;
			source = AudioController.Play(info.Buffer, inGame, defaultVolume * distanceVolume(), defaultPitch, vector, loops);
		}

		public void SetVolume(float volume)
		{
			source.SetVolume(defaultVolume * volume * distanceVolume(), Settings.EffectsVolume * Settings.MasterVolume);
		}

		public void SetPitch(float pitch)
		{
			source.SetPitch(defaultPitch * pitch);
		}

		public void SetPosition(CPos position)
		{
			var vector = convert(position);
			dist = vector.Dist;
			source.SetPosition(vector);
		}

		float distanceVolume()
		{
			return 1 / (1 + dist * reduction * 16);
		}

		Vector convert(CPos position)
		{
			if (inGame)
			{
				position -= Camera.LookAt;
				return position.ToVector() / new Vector(Camera.CurrentZoom * reduction, Camera.CurrentZoom * reduction, 1);
			}

			return position.ToVector() / new Vector(Camera.DefaultZoom * reduction, Camera.DefaultZoom * reduction, 1);
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
