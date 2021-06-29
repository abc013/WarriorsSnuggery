namespace WarriorsSnuggery.Audio
{
	public class MusicController
	{
		public readonly (string name, string file)[] data;
		readonly bool hasMusic;

		int current = 0;
		Music currentMusic;

		public bool LoopsSong { get; private set; }

		public MusicController(string[] names)
		{
			data = new (string, string)[names.Length];
			for (int i = 0; i < names.Length; i++)
				data[i] = (names[i], FileExplorer.Misc + "music" + FileExplorer.Separator + names[i] + ".wav");

			hasMusic = data.Length != 0;
		}

		public void SetVolume()
		{
			if (hasMusic && currentMusic != null)
				currentMusic.SetVolume();
		}

		public void LoopSong(string music)
		{
			LoopsSong = true;

			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].name == music)
				{
					current = i;

					Next();

					return;
				}
			}

			throw new System.Exception($"Unable to find specified song named {music}.");
		}

		public void LoopAll()
		{
			LoopsSong = false;

			Next();
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

			currentMusic = new Music(data[current].file);
			currentMusic.Play();

			if (!LoopsSong)
			{
				current++;

				if (current == data.Length)
					current = 0;
			}
		}
	}
}
