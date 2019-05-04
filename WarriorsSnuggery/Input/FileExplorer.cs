/*
 * User: Andreas
 * Date: 06.01.2018
 * 
 */
using System;
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
			while(MainDirectory == null)
			{
				if (!Directory.Exists(path))
					throw new DirectoryNotFoundException(path);

				if (find(path, @"\WS.yaml"))
					MainDirectory = path + @"\";
				else
					path += @"\..";
			}

			Misc = path + @"\misc\";
			Rules = path + @"\rules\";
			Maps = path + @"\maps\";
			Shaders = path + @"\shaders\";
			Logs = path + @"\logs\";
			Saves = path + @"\saves\";
		}

		static bool find(string path, string file)
		{
			foreach(string str in Directory.GetFiles(path))
			{
				if (str == path + file)
					return true;
			}

			return false;
		}

		public static bool CheckDll()
		{
			var files = Directory.GetFiles(Path);

			foreach(var raw in files)
			{
				var file = raw.Substring(raw.LastIndexOf('\\') + 1);
				if (file == "OpenTK.dll")
					return true;
			}

			return false;
		}

		/// <summary>
		/// Finds the directory in which the given file is.
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

			foreach(var file in Directory.EnumerateFiles(path))
			{
				if (!file.EndsWith(suffix, StringComparison.CurrentCulture))
					continue;

				if (file == pathName)
					return path;
			}

			var directories = Directory.EnumerateDirectories(path);

			foreach(var directory in directories)
			{
				var found = findUntil(directory + @"\", name, suffix);

				if (found != String.Empty)
					return found;
			}

			return string.Empty;
		}

		public static void CreateFile(string path, string name, string end = ".txt")
		{
			File.Create(path + name + end).Close();
		}
	}
}