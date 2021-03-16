using System.Collections.Generic;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

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

		public readonly GameAudioBuffer Buffer;

		public SoundType(List<TextNode> nodes, bool isDocumentation = false)
		{
			TypeLoader.SetValues(this, nodes);

			if (!isDocumentation)
			{
				AudioManager.LoadSound(Name, FileExplorer.FindPath(FileExplorer.Misc, Name, ".wav"));
				Buffer = AudioManager.GetBuffer(Name);
			}
		}
	}

	public class Sound
	{
		readonly SoundType info;
		readonly bool inGame;
		readonly float defaultVolume;
		readonly float defaultPitch;
		GameAudioSource source;
		float dist;

		public Sound(SoundType info, bool inGame = true)
		{
			this.info = info;
			this.inGame = inGame;
			defaultVolume = info.Volume + info.RandomVolume * (float)(Program.SharedRandom.NextDouble() - 0.5);
			defaultPitch = info.Pitch + info.RandomPitch * (float)(Program.SharedRandom.NextDouble() - 0.5);
		}

		public void Play(CPos position, bool loops, bool overwrite = true)
		{
			var vector = convert(position);
			dist = vector.Dist;

			if (source != null)
			{
				if (overwrite)
					Stop();
				else
					return;
			}
			
			source = AudioController.Play(info.Buffer, inGame, defaultVolume * distanceVolume(), defaultPitch, vector, loops);
		}

		public void SetVolume(float volume)
		{
			if (source == null)
				return;

			source.SetVolume(defaultVolume * volume * distanceVolume(), Settings.EffectsVolume * Settings.MasterVolume);
		}

		public void SetPitch(float pitch)
		{
			if (source == null)
				return;

			source.SetPitch(defaultPitch * pitch);
		}

		public void SetPosition(CPos position)
		{
			if (source == null)
				return;

			var vector = convert(position);
			dist = vector.Dist;
			source.SetPosition(vector);
		}

		float distanceVolume()
		{
			return 1 / (1 + dist);
		}

		Vector convert(CPos position)
		{
			if (inGame)
			{
				position -= Camera.LookAt;
				return position.ToVector() / new Vector(Camera.CurrentZoom, Camera.CurrentZoom, 1);
			}

			return position.ToVector() / new Vector(Camera.DefaultZoom, Camera.DefaultZoom, 1);
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
