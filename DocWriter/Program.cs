using System;
using System.IO;
using System.Linq;

namespace WarriorsSnuggery
{
	public enum ObjectType
	{
		ALL,
		ACTORS,
		PARTICLES,
		WEAPONS,
		TERRAIN,
		WALLS
	}

	static class Program
	{
		static void Main(string[] args)
		{
			FileExplorer.InitPaths();
			Console.ForegroundColor = ConsoleColor.White;

			Console.WriteLine("Welcome to the WarriorsSnuggery DocWriter. This program will search through the installation and create a documentation of the modding rules.");
			Console.WriteLine("Available is: ALL, ACTORS, PARTICLES, WEAPONS, TERRAIN and WALLS");

			Console.ForegroundColor = ConsoleColor.Yellow;

			Console.Write("Please enter what content should be created (e.g. ACTORS, TERRAIN): ");
			Console.ResetColor();

			var input = Console.ReadLine();

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("--------------------------------------------------------------");

			ObjectType[] types;
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

			try
			{
				using (var writer = new StreamWriter(FileExplorer.MainDirectory + "Documentation.html"))
				{
					Console.WriteLine("Generating document, please wait...");
					HTMLWriter.WriteHead(writer);

					foreach (var type in types)
					{
						Console.ForegroundColor = ConsoleColor.White;
						Console.WriteLine("Reading " + type + "...");
						Console.ResetColor();
						HTMLWriter.WriteDoc(writer, type);
					}

					HTMLWriter.WriteEnd(writer);

					writer.Flush();
					writer.Close();
				}
			}
			catch (Exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Failed to init document/write rules.");
				Console.ReadKey();
				return;
			}

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("--------------------------------------------------------------");
			Console.WriteLine("Done!");
			Console.ReadKey();
		}

		static ObjectType[] getTypes(string input)
		{
			var strings = input.Split(',');

			var types = new ObjectType[strings.Length];
			for (int i = 0; i < types.Length; i++)
			{
				types[i] = (ObjectType)Enum.Parse(typeof(ObjectType), strings[i].Trim(), true);
			}
			if (types.Contains(ObjectType.ALL))
				types = new ObjectType[] { ObjectType.ACTORS, ObjectType.PARTICLES, ObjectType.WEAPONS, ObjectType.WALLS, ObjectType.TERRAIN };

			return types;
		}
	}
}
