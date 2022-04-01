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

		public readonly string Directory;
		public readonly List<TextNode> Rules;

		public Mod(string filepath)
		{
			Directory = FileExplorer.FileDirectory(filepath);

			Rules = TextNodeLoader.FromFile(Directory, FileExplorer.FileName(filepath) + FileExplorer.FileExtension(filepath));

			TypeLoader.SetValues(this, Rules.Find(n => n.Key == "Mod").Children);
		}
	}
}
