using System;

namespace WarriorsSnuggery.Audio
{
	public static class MusicController
	{
		static (string name, string file)[] data;
		static bool hasMusic;

		static Music currentMusic;
		static int current = 0;

		static bool intenseActive;
		static int intenseDuration;
		static float intenseVolume;
		static Music currentIntenseMusic;
		static int currentIntense = 0;

		public static bool SongLooping { get; private set; }

		public static void Load()
		{
			// TODO
			var files = FileExplorer.FilesIn(PackageManager.Core.ContentDirectory + "music" + FileExplorer.Separator, ".wav");

			data = new (string, string)[files.Length];
			for (int i = 0; i < files.Length; i++)
				data[i] = (FileExplorer.FileName(files[i]), files[i]);

			hasMusic = data.Length != 0;
		}

		public static void UpdateVolume()
		{
			if (!hasMusic)
				return;

			currentMusic.UpdateVolume();
			if (intenseActive)
				currentIntenseMusic.UpdateVolume();
		}

		public static void LoopSong(string music, string intenseMusic = null)
		{
			SongLooping = true;
			intenseActive = !string.IsNullOrEmpty(intenseMusic);

			current = findMusic(music);
			if (intenseActive)
				currentIntense = findMusic(intenseMusic);

			NextSong();
		}

		static int findMusic(string music)
		{
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].name == music)
					return i;
			}

			throw new Exception($"Unable to find specified song named {music}.");
		}

		public static void LoopAllSongs()
		{
			SongLooping = false;
			intenseActive = false;

			NextSong();
		}

		public static void NextSong()
		{
			if (!hasMusic || AudioController.MusicSource == null)
				return;

			nextSong(AudioController.MusicSource, ref currentMusic, ref current);
			currentMusic.SetVolume(1f - intenseVolume);

			if (intenseActive)
			{
				nextSong(AudioController.IntenseMusicSource, ref currentIntenseMusic, ref currentIntense);
				currentIntenseMusic.SetVolume(intenseVolume);
			}
			else if (currentIntenseMusic != null)
			{
				currentIntenseMusic.Dispose();
				currentIntenseMusic = null;
			}
		}

		static void nextSong(MusicAudioSource source, ref Music music, ref int index)
		{
			music?.Dispose();

			music = new Music(data[index].file, SongLooping);
			music.Play(source);

			if (!SongLooping && ++index == data.Length)
				index = 0;
		}

		public static void Tick()
		{
			if (!hasMusic || AudioController.MusicSource == null)
				return;

			currentMusic.Tick();
			if (currentMusic.Done)
				nextSong(AudioController.MusicSource, ref currentMusic, ref current);

			if (intenseActive)
			{
				if (intenseDuration-- > 0)
					setIntenseVolume(Math.Min(intenseVolume + 0.008f, 1f));
				else
					setIntenseVolume(Math.Max(intenseVolume - 0.002f, 0f));

				currentIntenseMusic.Tick();
				if (currentIntenseMusic.Done)
					nextSong(AudioController.IntenseMusicSource, ref currentIntenseMusic, ref currentIntense);
			}
		}

		public static void FadeIntenseIn(int duration)
		{
			intenseDuration = duration;
		}

		public static void FadeIntenseOut()
		{
			intenseDuration = 0;
		}

		public static void ResetIntense()
		{
			intenseDuration = 0;
			intenseVolume = 0;
		}

		static void setIntenseVolume(float intense)
		{
			if (!hasMusic || !intenseActive)
				return;

			if (intense < 0 || intense > 1)
				throw new ArgumentOutOfRangeException($"Music volume mix can only be set in the range from 0 to 1 (attempt: {intense})");

			intenseVolume = intense;
			currentMusic.SetVolume(1f - intense);
			currentIntenseMusic.SetVolume(intense);
		}
	}
}
