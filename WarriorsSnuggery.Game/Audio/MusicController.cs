using System;

namespace WarriorsSnuggery.Audio
{
	public static class MusicController
	{
		static  (string name, string file)[] data;
		static bool hasMusic;

		static bool intenseActive;

		static Music currentMusic;
		static int current = 0;

		static Music currentIntenseMusic;
		static int currentIntense = 0;

		public static bool SongLooping { get; private set; }

		public static void Load()
		{
			var fileNames = FileExplorer.FilesIn(FileExplorer.Misc + "music" + FileExplorer.Separator, ".wav");

			data = new (string, string)[fileNames.Length];
			for (int i = 0; i < fileNames.Length; i++)
				data[i] = (fileNames[i], FileExplorer.Misc + "music" + FileExplorer.Separator + fileNames[i] + ".wav");

			hasMusic = data.Length != 0;
		}

		public static void UpdateVolume()
		{
			if (hasMusic)
			{
				currentMusic?.UpdateVolume();
				if (intenseActive)
					currentIntenseMusic.UpdateVolume();
			}
		}

		public static void LoopSong(string music, string intenseMusic = null)
		{
			SongLooping = true;
			intenseActive = !string.IsNullOrEmpty(intenseMusic);

			current = findMusic(music);
			if (intenseActive)
				currentIntense = findMusic(intenseMusic);
			else if (currentIntenseMusic != null)
			{
				currentIntenseMusic.Dispose();
				currentIntenseMusic = null;
			}

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

			NextSong();
		}

		public static void NextSong()
		{
			currentMusic = nextSong(currentMusic, AudioController.MusicSource, ref current);

			if (intenseActive)
				currentIntenseMusic = nextSong(currentIntenseMusic, AudioController.IntenseMusicSource, ref currentIntense);
		}

		static Music nextSong(Music music, MusicAudioSource source, ref int index)
		{
			if (!hasMusic)
				return null;

			if (music != null)
				music.Dispose();

			music = new Music(data[index].file, SongLooping);
			music.Play(source);

			if (!SongLooping)
			{
				index++;

				if (index == data.Length)
					index = 0;
			}

			return music;
		}

		public static void Tick()
		{
			if (!hasMusic)
				return;

			if (currentMusic != null)
			{
				currentMusic.Tick();

				if (currentMusic.Done)
					currentMusic = nextSong(currentMusic, AudioController.MusicSource, ref current);
			}

			if (intenseActive)
			{
				currentIntenseMusic.Tick();

				if (currentIntenseMusic.Done)
					currentIntenseMusic = nextSong(currentIntenseMusic, AudioController.IntenseMusicSource, ref currentIntense);
			}
		}

		public static void SetIntenseVolume(float normal)
		{
			if (!hasMusic || !intenseActive)
				return;

			if (normal < 0 || normal > 1)
				throw new ArgumentOutOfRangeException($"Music volume mix can only be set in the range from 0 to 1 (attempt: {normal})");

			currentMusic.SetVolume(normal);
			currentIntenseMusic.SetVolume(1f - normal);
		}
	}
}
