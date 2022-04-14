using System;
using System.Linq;

namespace WarriorsSnuggery.Docs
{
	static class Program
	{
		public const string Title = WarriorsSnuggery.Program.Title + " - Rule Documentation";

		public static bool SkipHeader;

		public static void Main(string[] args)
		{
			Console.Title = Title;

			SkipHeader = args.Contains("-skip-header");

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

			Console.WriteLine("Generating document, please wait...");
			DocumentWriter.BeginDocument();
			IndexWriter.BeginIndex();

			// First generate the chapters...
			var chapters = new ChapterWriter[types.Length];
			for (int i = 0; i < types.Length; i++)
			{
				var type = types[i];

				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine($"Reading {type}...");
				Console.ResetColor();

				var chapter = new ChapterWriter(type, i + 1);
				chapter.WriteDocumentation();
				chapters[i] = chapter;
			}

			IndexWriter.EndIndex();

			// Then write index and then chapters
			DocumentWriter.WriteIndex();
			foreach (var chapter in chapters)
				DocumentWriter.WriteChapter(chapter);

			DocumentWriter.EndDocument();
		}
	}
}
