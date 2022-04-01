using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public class ModManager
	{
		public static Mod Core;

		public static readonly List<Mod> Mods = new List<Mod>();

		public static void Load()
		{
			Core = new Mod(FileExplorer.MainDirectory);

			foreach (var directory in FileExplorer.DirectoriesIn(FileExplorer.Mods))
			{
				Mods.Add(new Mod(directory));
			}
		}

		public static void Reload()
		{
			Mods.Clear();

			Load();
		}
	}
}
