using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery
{
	public static class PackageManager
	{
		public static Package Core;
		public static readonly List<Package> AvailablePackages = new List<Package>();
		public static readonly List<Package> ActivePackages = new List<Package>();

		public static void Load()
		{
			Core = new Package(FileExplorer.Core + "Rules.yaml");

			foreach (var directory in FileExplorer.DirectoriesIn(FileExplorer.Mods))
			{
				var filepath = directory + FileExplorer.Separator + "Rules.yaml";

				if (File.Exists(filepath))
					AvailablePackages.Add(new Package(filepath));
			}

			ActivePackages.Add(Core);

			var unknownPackages = new List<string>();
			foreach (var name in Settings.PackageList)
			{
				var package = AvailablePackages.FirstOrDefault(p => p.InternalName == name);

				if (package != null)
				{
					ActivePackages.Add(package);
					if (package.Outdated)
						Log.LoaderWarning("Mods", $"Enabling outdated package '{package.InternalName}' (Version '{package.GameVersion}').");
					else
						Log.LoaderDebug("Mods", $"Enabling package '{package.InternalName}'.");
				}
				else
				{
					unknownPackages.Add(name);
					Log.LoaderWarning("Mods", $"Unable to fetch unknown package '{name}'. Removing and skipping.");
				}
			}

			foreach (var package in unknownPackages)
				Settings.PackageList.Remove(package);
		}

		public static void Reload()
		{
			AvailablePackages.Clear();

			Load();
		}
	}
}
