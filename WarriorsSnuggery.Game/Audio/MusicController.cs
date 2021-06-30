namespace WarriorsSnuggery.Audio
{
	public static class MusicController
	{
		static  (string name, string file)[] data;
		static bool hasMusic;

		static int current = 0;

		static Music currentMusic;

		public static bool LoopsSong { get; private set; }

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
			if (hasMusic && currentMusic != null)
				currentMusic.UpdateVolume();
		}

		public static void LoopSong(string music)
		{
			LoopsSong = true;

			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].name == music)
				{
					current = i;

					NextSong();

					return;
				}
			}

			throw new System.Exception($"Unable to find specified song named {music}.");
		}

		public static void LoopAllSongs()
		{
			LoopsSong = false;

			NextSong();
		}

		public static void NextSong()
		{
			if (!hasMusic)
				return;

			if (currentMusic != null)
			{
				currentMusic.Stop();
				currentMusic.Dispose();
			}

			currentMusic = new Music(data[current].file);
			currentMusic.Play();

			if (!LoopsSong)
			{
				current++;

				if (current == data.Length)
					current = 0;
			}
		}

		public static void Tick()
		{
			if (!hasMusic)
				return;

			if (currentMusic != null)
			{
				currentMusic.Tick();

				if (currentMusic.Done)
					NextSong();
			}
		}
	}
}
