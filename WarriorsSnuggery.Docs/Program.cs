using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery.Docs
{
	static class Program
	{
		public const string Title = WarriorsSnuggery.Program.Title + " - Rule Documentation";

		public static bool DarkMode;
		public static void Main()
		{
			Console.Title = Title;

			FileExplorer.InitPaths();
			TypeWriter.Initialize();

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("Welcome to the WarriorsSnuggery DocWriter. This program will search through the installation and create a documentation of the modding rules.");

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("Write the documentation in dark mode? (y to accept): ");
			Console.ResetColor();

			DarkMode = Console.ReadKey().Key == ConsoleKey.Y;
			HTMLWriter.Colors = DarkMode ? HTMLWriter.DarkModeColors : HTMLWriter.LightModeColors;
			Console.WriteLine();

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("--------------------------------------------------------------");

			try
			{
				start();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Failed to write rules. Exception:");
				Console.WriteLine(e);
				Console.ReadKey();
				return;
			}

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("--------------------------------------------------------------");
			Console.WriteLine("Done!");
			Console.ReadKey();
		}

		static void start()
		{
			var types = Enum.GetValues<DocumentationType>();
			using var writer = new StreamWriter(FileExplorer.MainDirectory + "Documentation.html");

			HTMLWriter.SetWriter(writer);

			Console.WriteLine("Generating document, please wait...");
			HTMLWriter.WriteHead();

			HTMLWriter.WriteIndex(types);

			int id = 1;
			foreach (var type in types)
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Reading " + type + "...");
				Console.ResetColor();
				HTMLWriter.WriteDoc(type, id++);
			}

			HTMLWriter.WriteEnd();

			writer.Flush();
			writer.Close();
		}
	}
}
