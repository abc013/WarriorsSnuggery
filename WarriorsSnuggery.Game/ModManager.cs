using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery
{
	public static class ModManager
	{
		public static Mod Core;
		public static readonly List<Mod> AvailableMods = new List<Mod>();
		public static readonly List<Mod> ActiveMods = new List<Mod>();

		public static void Load()
		{
			Core = new Mod(FileExplorer.Core + "Rules.yaml");

			foreach (var directory in FileExplorer.DirectoriesIn(FileExplorer.Mods))
			{
				var filepath = directory + FileExplorer.Separator + "Rules.yaml";

				if (File.Exists(filepath))
					AvailableMods.Add(new Mod(filepath));
			}

			ActiveMods.Add(Core);
			foreach (var name in Settings.ModList)
			{
				var mod = AvailableMods.FirstOrDefault(m => m.InternalName == name);

				if (mod != null)
				{
					ActiveMods.Add(mod);
					if (mod.Outdated)
						Log.LoaderWarning("Mods", $"Enabling outdated mod '{mod.InternalName}' (Version '{mod.GameVersion}').");
					else
						Log.LoaderDebug("Mods", $"Enabling mod '{mod.InternalName}'.");
				}
				else
					Log.Warning($"Unable to fetch unknown mod '{name}'. Skipping.");
			}
		}

		public static void Reload()
		{
			AvailableMods.Clear();

			Load();
		}
	}
}
