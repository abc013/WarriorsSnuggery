using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery
{
	public class Mod
	{
		[Require]
		public readonly string Name;
		[Require]
		public readonly string InternalName;

		public readonly string Description = "No description.";
		public readonly string Author = "Unknown author.";
		public readonly string Version = "Dev";
		public readonly string GameVersion = "Unknown";
		public bool Outdated => GameVersion != Settings.Version;

		public readonly string Directory;
		public readonly List<TextNode> Rules;

		public Mod(string filepath)
		{
			Directory = FileExplorer.FileDirectory(filepath);

			var rules = TextNodeLoader.FromFile(Directory, FileExplorer.FileName(filepath) + FileExplorer.FileExtension(filepath));

			TypeLoader.SetValues(this, rules.Find(n => n.Key == "Mod").Children);
			Rules = rules.Find(n => n.Key == "Rules").Children;
		}
	}
}
