namespace WarriorsSnuggery.Audio
{
	public class MusicController
	{
		public readonly Music[] music;
		int current = 0;

		public MusicController(string[] names)
		{
			music = new Music[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				AudioManager.LoadSound(names[i], FileExplorer.Misc + @"music\");
				music[i] = new Music(names[i]);
			}

			if (music.Length != 0)
				music[current].Play();
		}

		public void SetVolume()
		{
			if (music.Length != 0)
				music[current].SetVolume();
		}

		public void Tick()
		{
			if (music.Length == 0)
				return;

			music[current].Tick();
			if (music[current].Done)
				Next();
		}

		public void Next()
		{
			if (music.Length == 0)
				return;

			music[current].Stop();

			current++;
			if (current == music.Length)
				current = 0;

			music[current].Play();
		}
	}
}
