using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WarriorsSnuggery;

namespace DocWriter
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
			Console.WriteLine("Available are: ALL, ACTORS, PARTICLES, WEAPONS, TERRAIN, WALLS, MAPS, SPELLS, TROPHIES and SOUNDS.");

			Console.ForegroundColor = ConsoleColor.Yellow;

			Console.Write("Please enter what content should be created (e.g. ACTORS, TERRAIN): ");
			Console.ResetColor();

			var input = Console.ReadLine();

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("Write the documentation in dark mode? (y to accept): ");
			Console.ResetColor();

			DarkMode = Console.ReadKey().Key == ConsoleKey.Y;
			HTMLWriter.Colors = DarkMode ? HTMLWriter.DarkModeColors : HTMLWriter.LightModeColors;
			Console.WriteLine();

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("--------------------------------------------------------------");

			DocumentationType[] types;
			try
			{
				types = getTypes(input);
			}
			catch (Exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Invalid input.");
				Console.ReadKey();
				return;
			}

			if (Debugger.IsAttached)
				start(types);
			else
			{
				try
				{
					start(types);
				}
				catch (Exception)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Failed to init document/write rules.");
					Console.ReadKey();
					return;
				}
			}

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("--------------------------------------------------------------");
			Console.WriteLine("Done!");
			Console.ReadKey();
		}

		static void start(DocumentationType[] types)
		{
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

		static DocumentationType[] getTypes(string input)
		{
			var strings = input.Split(',');

			var types = new DocumentationType[strings.Length];
			for (int i = 0; i < types.Length; i++)
				types[i] = (DocumentationType)Enum.Parse(typeof(DocumentationType), strings[i].Trim(), true);

			if (types.Contains(DocumentationType.ALL))
				types = new DocumentationType[] { DocumentationType.ACTORS, DocumentationType.PARTICLES, DocumentationType.WEAPONS, DocumentationType.WALLS, DocumentationType.TERRAIN, DocumentationType.MAPS, DocumentationType.SPELLS, DocumentationType.SOUNDS };

			return types;
		}
	}
}
