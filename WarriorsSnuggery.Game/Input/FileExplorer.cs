using System;
using System.Collections.Generic;
using System.IO;

namespace WarriorsSnuggery
{
	public static class FileExplorer
	{
		public static readonly string Path = Directory.GetCurrentDirectory();
		public static readonly char Separator = System.IO.Path.DirectorySeparatorChar;
		public static readonly char[] InvalidFileChars = System.IO.Path.GetInvalidFileNameChars();

		public static string Misc;
		public static string Rules;
		public static string Pieces;
		public static string Scripts;
		public static string Shaders;
		public static string Logs;
		public static string Saves;
		public static string MainDirectory;

		public static void InitPaths()
		{
			MainDirectory = AppContext.BaseDirectory;

			Misc = MainDirectory + "misc" + Separator;
			Rules = MainDirectory + "rules" + Separator;
			Pieces = MainDirectory + "pieces" + Separator;
			Scripts = MainDirectory + "scripts" + Separator;
			Shaders = MainDirectory + "shaders" + Separator;
			Logs = MainDirectory + "logs" + Separator;
			Saves = MainDirectory + "saves" + Separator;
		}

		public static bool Exists(string path, string name)
		{
			return File.Exists(path + name);
		}

		/// <summary>
		/// Finds the whole path of the given file.
		/// </summary>
		/// <returns>Returns the filepath.</returns>
		public static string FindIn(string path, string name, string suffix = ".png")
		{
			if (name == string.Empty)
				return string.Empty;

			name += suffix;

			var found = findUntil(path, name, suffix);

			if (found == string.Empty)
				throw new FileNotFoundException("The file " + name + " has not been found");

			return found + name;
		}

		/// <summary>
		/// Finds the directory in which the given file is.
		/// </summary>
		/// <returns>Returns the directory.</returns>
		public static string FindPath(string path, string name, string suffix = ".png")
		{
			if (name == string.Empty)
				return string.Empty;

			name += suffix;

			var found = findUntil(path, name, suffix);

			if (found == string.Empty)
				throw new FileNotFoundException("The file " + name + " has not been found");

			return found;
		}

		static string findUntil(string path, string name, string suffix)
		{
			var pathName = path + name;

			foreach (var file in Directory.EnumerateFiles(path))
			{
				if (!file.EndsWith(suffix, StringComparison.CurrentCulture))
					continue;

				if (file == pathName)
					return path;
			}

			foreach (var directory in Directory.EnumerateDirectories(path))
			{
				var found = findUntil(directory + Separator, name, suffix);

				if (found != string.Empty)
					return found;
			}

			return string.Empty;
		}

		public static string[] FilesIn(string path, string suffix = ".yaml")
		{
			var list = new List<string>();
			foreach (var file in Directory.EnumerateFiles(path))
			{
				if (!file.EndsWith(suffix, StringComparison.CurrentCulture))
					continue;

				var split = file.Split(Separator);
				var name = split[^1];
				list.Add(name.Remove(name.Length - suffix.Length));
			}

			return list.ToArray();
		}

		public static void WriteScreenshot(byte[] array, int width, int height)
		{
			var file = Logs + "screenshot_" + DateTime.Now.ToString("HHmmss_ddMMyyyy") + ".png";
			Loader.BitmapSaver.Save(file, array, new MPos(width, height), true);
		}
	}
}