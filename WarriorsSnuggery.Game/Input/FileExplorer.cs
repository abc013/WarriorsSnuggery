using System;
using System.IO;

namespace WarriorsSnuggery
{
	public static class FileExplorer
	{
		public static readonly char Separator = Path.DirectorySeparatorChar;
		public static readonly char[] InvalidFileChars = Path.GetInvalidFileNameChars();

		public static string MainDirectory;

		public static string Shaders;
		public static string Logs;
		public static string Saves;
		public static string Core;
		public static string Mods;
		public static string Fonts;

		public static void InitPaths()
		{
			MainDirectory = AppContext.BaseDirectory;

			Core = MainDirectory + "core" + Separator;

			Shaders = MainDirectory + "shaders" + Separator;
			Logs = MainDirectory + "logs" + Separator;
			Saves = MainDirectory + "saves" + Separator;
			Mods = MainDirectory + "mods" + Separator;
			Fonts = MainDirectory + "fonts" + Separator;
		}

		public static Mod ResolveMod(string modAndFile)
		{
			var split = modAndFile.Split('|');

			if (split.Length == 1)
				return ModManager.Core;

			if (split.Length == 2)
				return ModManager.ActiveMods.Find(mod => mod.InternalName == split[0]);

			throw new InvalidDataException($"Filename contains multiple mod indicators '|'.");
		}

		public static string ResolveFile(string modAndFile)
		{
			var split = modAndFile.Split('|');

			if (split.Length == 1)
				return split[0];

			if (split.Length == 2)
				return split[1];

			throw new InvalidDataException($"Filename contains multiple mod indicators '|'.");
		}

		public static string FileName(string filepath)
		{
			return Path.GetFileNameWithoutExtension(filepath);
		}

		public static string FileExtension(string filepath)
		{
			return Path.GetExtension(filepath);
		}

		public static string FileDirectory(string filepath)
		{
			return Path.GetDirectoryName(filepath) + Separator;
		}

		/// <summary>
		/// Finds the the given file.
		/// </summary>
		/// <returns>Returns the path to the file.</returns>
		public static string FindIn(string path, string name, string extension)
		{
			var file = name + extension;

			var files = Directory.GetFiles(path, file, SearchOption.AllDirectories);

			if (files.Length == 0)
				throw new FileNotFoundException($"The file {file} has not been found.");
			if (files.Length > 1)
				throw new FileNotFoundException($"The file {file} is equivocal (Count: {files.Length}).");

			return files[0];
		}

		/// <summary>
		/// Finds the directory in which the given file is.
		/// </summary>
		/// <returns>Returns the directory.</returns>
		public static string FindPath(string path, string name, string extension)
		{
			return Path.GetDirectoryName(FindIn(path, name, extension)) + Separator;
		}

		/// <summary>
		/// Finds all directories in a directory.
		/// </summary>
		/// <returns>Returns the directories.</returns>
		public static string[] DirectoriesIn(string path, bool recurse = false)
		{
			return Directory.GetDirectories(path, "*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		}

		/// <summary>
		/// Finds all files in a directory.
		/// </summary>
		/// <returns>Returns the path to the file.</returns>
		public static string[] FilesIn(string path, string extension, bool recurse = false)
		{
			return Directory.GetFiles(path, $"*{extension}", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		}

		public static void WriteScreenshot(byte[] array, int width, int height)
		{
			var file = Logs + "screenshot_" + DateTime.Now.ToString("HHmmss_ddMMyyyy") + ".png";
			Loader.BitmapSaver.Save(file, array, new MPos(width, height), true);
		}
	}
}