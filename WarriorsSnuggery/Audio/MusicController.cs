using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarriorsSnuggery.Audio
{
	public class MusicController
	{
		public readonly Music[] music;
		int current = 0;

		public float Volume = 1f;

		public MusicController(string[] names)
		{
			music = new Music[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				AudioManager.LoadSound(names[i], FileExplorer.FindPath(FileExplorer.Misc, names[i], ".wav"));
				music[i] = new Music(names[i]);
			}

			music[current].Play(Volume);
		}

		public void Tick()
		{
			music[current].Tick();
			if (music[current].Done)
				Next();
		}

		public void Next()
		{
			music[current].Stop();

			current++;
			if (current == music.Length)
				current = 0;

			music[current].Play(Volume);
		}
	}
}
