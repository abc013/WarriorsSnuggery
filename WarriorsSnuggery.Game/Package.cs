using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery
{
	public class Package
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
		public string ContentDirectory => Directory + "contents" + FileExplorer.Separator;
		public string RulesDirectory => Directory + "rules" + FileExplorer.Separator;
		public string PiecesDirectory => Directory + "pieces" + FileExplorer.Separator;
		public string ScriptsDirectory => Directory + "scripts" + FileExplorer.Separator;

		public readonly List<TextNode> Rules;

		public Package(string filepath)
		{
			Directory = FileExplorer.FileDirectory(filepath);

			var rules = TextNodeLoader.FromFilepath(filepath);

			TypeLoader.SetValues(this, rules.Find(n => n.Key == "Package").Children);
			Rules = rules.Find(n => n.Key == "Rules").Children;
		}

		public override string ToString()
		{
			return InternalName;
		}
	}
}
