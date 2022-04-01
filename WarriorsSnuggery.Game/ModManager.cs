using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery
{
	public class ModManager
	{
		public static Mod Core;

		public static readonly List<Mod> Mods = new List<Mod>();

		public static void Load()
		{
			// Core = new Mod(FileExplorer.MainDirectory);

			foreach (var directory in FileExplorer.DirectoriesIn(FileExplorer.Mods))
			{
				var filepath = directory + FileExplorer.Separator + "Rules.yaml";

				if (File.Exists(filepath))
					Mods.Add(new Mod(filepath));
			}
		}

		public List<Mod> GetActiveMods()
		{
			var mods = new List<Mod>();

			foreach (var name in Settings.ModList)
			{
				var mod = Mods.FirstOrDefault(m => m.InternalName == name);

				if (mod != null)
					mods.Add(mod);
				else
					Log.Warning($"Unable to fetch unknown mod '{name}'. Skipping.");
			}

			return mods;
		}

		public static void Reload()
		{
			Mods.Clear();

			Load();
		}
	}
}
