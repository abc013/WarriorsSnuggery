using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public static class GameSaveManager
	{
		public const string DefaultSaveName = "DEFAULT";
		public static GameSave DefaultSave;

		public static readonly List<GameSave> Saves = new List<GameSave>();

		public static void Load()
		{
			foreach (var file in FileExplorer.FilesIn(FileExplorer.Saves, ".yaml"))
			{
				if (file != DefaultSaveName && !file.EndsWith("_map.yaml")) //make sure that we don't add any maps
					Saves.Add(new GameSave(file));
			}

			DefaultSave = new GameSave(FileExplorer.FindIn(FileExplorer.Saves, DefaultSaveName, ".yaml"));
		}

		public static void Reload()
		{
			Saves.Clear();

			Load();
		}

		public static void Delete(GameSave save)
		{
			save.Delete();

			Reload();
		}

		public static void SaveOnNewName(GameSave save, string name, Game game)
		{
			save.SetName(name);
			save.Save(game);

			Reload();
		}
	}
}
