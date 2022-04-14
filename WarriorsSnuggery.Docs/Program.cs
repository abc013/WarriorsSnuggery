using System;
using System.IO;

namespace WarriorsSnuggery.Docs
{
	static class Program
	{
		public const string Title = WarriorsSnuggery.Program.Title + " - Rule Documentation";

		public static void Main()
		{
			Console.Title = Title;

			FileExplorer.InitPaths();
			TypeWriter.Initialize();

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("Welcome to the WarriorsSnuggery DocWriter. This program will search through the installation and create a documentation of the modding rules.");
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
		}

		static void start()
		{
			var types = Enum.GetValues<DocumentationType>();
			using var writer = new StreamWriter(FileExplorer.MainDirectory + "Documentation.html");

			Console.WriteLine("Generating document, please wait...");
			HTMLWriter.BeginDocument(writer);

			HTMLWriter.WriteIndex();

			int id = 1;
			foreach (var type in types)
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Reading " + type + "...");
				Console.ResetColor();
				HTMLWriter.WriteDoc(type, id++);
			}

			HTMLWriter.EndDocument();

			writer.Flush();
			writer.Close();
		}
	}
}
