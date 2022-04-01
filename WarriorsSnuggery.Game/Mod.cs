namespace WarriorsSnuggery
{
	public class Mod
	{
		public readonly string Name;
		public readonly string InternalName;

		public readonly string Description = "No description.";
		public readonly string Author = "Unknown author.";
		public readonly string Version = "v?";
		public readonly string GameVersion = "Unknown";

		public readonly string Directory;

		public Mod(string directory)
		{
			Directory = directory;
			InternalName = FileExplorer.FileName(directory);
			Name = FileExplorer.FileName(directory);

			// TODO
		}
	}
}
