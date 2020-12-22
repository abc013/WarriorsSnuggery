namespace WarriorsSnuggery.Audio
{
	public class MusicController
	{
		public readonly string[] files;
		readonly bool hasMusic;

		int current = 0;
		Music currentMusic;

		public MusicController(string[] names)
		{
			files = new string[names.Length];
			for (int i = 0; i < names.Length; i++)
				files[i] = FileExplorer.Misc + @"music\" + names[i] + ".wav";

			hasMusic = files.Length != 0;

			if (hasMusic)
				Next();
		}

		public void SetVolume()
		{
			if (hasMusic && currentMusic != null)
				currentMusic.SetVolume();
		}

		public void Tick()
		{
			if (!hasMusic)
				return;

			if (currentMusic != null)
			{
				currentMusic.Tick();
				if (currentMusic.Done)
					Next();
			}
		}

		public void Next()
		{
			if (!hasMusic)
				return;

			if (currentMusic != null)
			{
				currentMusic.Stop();
				currentMusic.Dispose();
			}

			currentMusic = new Music(files[current++]);
			currentMusic.Play();

			if (current == files.Length)
				current = 0;
		}
	}
}
