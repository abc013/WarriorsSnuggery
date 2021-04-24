﻿using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public static class GameSaveManager
	{
		public static GameSave DefaultStatistic;

		public static readonly List<GameSave> Saves = new List<GameSave>();

		public static void Load()
		{
			foreach (var file in FileExplorer.FilesIn(FileExplorer.Saves))
			{
				if (!file.EndsWith("_map")) //make sure that we don't add any maps
					Saves.Add(new GameSave(file));
			}
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
			save.Save(game.World);

			Reload();
		}
	}
}
