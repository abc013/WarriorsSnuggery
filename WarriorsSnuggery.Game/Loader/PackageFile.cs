using System.IO;

namespace WarriorsSnuggery.Loader
{
	public class PackageFile
	{
		readonly string packageFile;

		public readonly string File;
		public readonly Package Package;

		public PackageFile(Package package, string file)
		{
			if (package == PackageManager.Core)
				packageFile = file;
			else
				packageFile = package.InternalName + "|" + file;

			File = file;
			Package = package;
		}

		public PackageFile(string packageFile)
		{
			this.packageFile = packageFile;

			var split = packageFile.Split('|');
			if (split.Length == 1)
			{
				Package = PackageManager.Core;
				File = split[0];
			}
			else if (split.Length == 2)
			{
				Package = PackageManager.ActivePackages.Find(package => package.InternalName == split[0]);
				File = split[1];
			}
			else
				throw new InvalidDataException($"Filename contains multiple package indicators '|'.");
		}

		public override string ToString()
		{
			return packageFile;
		}
	}
}
