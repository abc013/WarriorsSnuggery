using System;
using System.Collections.Generic;
using System.IO;

namespace WarriorsSnuggery
{
	public static class FileExplorer
	{
		public static readonly string Path = Directory.GetCurrentDirectory();

		public static string Misc;
		public static string Rules;
		public static string Maps;
		public static string Shaders;
		public static string Logs;
		public static string Saves;
		public static string MainDirectory;

		public static void InitPaths()
		{
			string path = Directory.GetCurrentDirectory();

			MainDirectory = path + @"\";
			Misc = path + @"\misc\";
			Rules = path + @"\rules\";
			Maps = path + @"\maps\";
			Shaders = path + @"\shaders\";
			Logs = path + @"\logs\";
			Saves = path + @"\saves\";
		}

		public static bool CheckDll()
		{
			var files = Directory.GetFiles(Path);

			foreach (var raw in files)
			{
				var file = raw.Substring(raw.LastIndexOf('\\') + 1);
				if (file == "OpenTK.dll")
					return true;
			}

			return false;
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
				var found = findUntil(directory + @"\", name, suffix);

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

				var split = file.Split('\\');
				var name = split[split.Length - 1];
				list.Add(name.Remove(name.Length - 4));
			}

			return list.ToArray();
		}

		public static FileStream CreateFile(string path, string name, string suffix = ".txt")
		{
			return File.Create(path + name + suffix);
		}

		public static void WriteScreenshot(float[] array, int width, int height)
		{
			var file = Logs + "screenshot_" + DateTime.Now.ToString("HHmmss_ddMMyyyy") + ".png";
			Loader.BitmapSaver.Save(file, array, new MPos(width, height), true);
		}
	}
}