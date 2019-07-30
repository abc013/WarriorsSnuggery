using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WarriorsSnuggery
{
	public static class HTMLWriter
	{
		public static void WriteHead(StreamWriter writer)
		{
			writer.WriteLine("<html>");
			writer.WriteLine("\t<head>");
			writer.WriteLine("\t\t<title>Rule Documentation</title>");
			WriteStyle(writer);
			writer.WriteLine("\t</head>");
			writer.WriteLine("\t<body>");
		}

		static void WriteStyle(StreamWriter writer)
		{
			writer.WriteLine("\t\t<style>");
			writer.WriteLine("table { margin: 10px; box-shadow: 1px 0px 10px #EEEEEE; width: 50%; border-collapse: collapse; }");
			writer.WriteLine("td { border: 1px solid #DDDDDD; padding: 8px; }");
			writer.WriteLine("th { border: 1px solid #DDDDDD; padding: 8px; }");
			writer.WriteLine("tr:nth-child(even) { background-color: #EEEEEE; }");
			writer.WriteLine("h1 { margin-bottom: 0px; }");
			writer.WriteLine("h2 { margin-bottom: 0px; }");
			writer.WriteLine("\t\t</style>");
		}

		public static void WriteDoc(StreamWriter writer, DocumentationType type)
		{
			writer.WriteLine();
			writer.WriteLine("\t\t<h1>" + type.ToString().ToLower() + "</h1>");
			writer.WriteLine("\t\t<hr>");
			writer.WriteLine();

			switch (type)
			{
				case DocumentationType.ACTORS:
					ObjectWriter.WriteActors(writer);

					break;
				case DocumentationType.PARTICLES:
					ObjectWriter.WriteParticles(writer);

					break;
				case DocumentationType.TERRAIN:
					ObjectWriter.WriteTerrain(writer);

					break;
				case DocumentationType.WALLS:
					ObjectWriter.WriteWalls(writer);

					break;
				case DocumentationType.WEAPONS:
					ObjectWriter.WriteWeapons(writer);

					break;
				case DocumentationType.MAPS:
					writer.WriteLine("\t\t<h2> Map Information</h2>");
					writer.WriteLine("\t\t<hr>");
					writer.WriteLine();
					ObjectWriter.WriteMaps(writer);
					writer.WriteLine();
					writer.WriteLine("\t\t<h2> Map Generators</h2>");
					writer.WriteLine("\t\t<hr>");
					writer.WriteLine();
					ObjectWriter.WriteMapGenerators(writer);

					break;
			}
		}

		public static void WriteRuleHead(StreamWriter writer, string rule, string[] description)
		{
			writer.WriteLine("\t\t<h2>" + rule + "</h2>");
			writer.WriteLine("\t\t<hr color=\"#CCCCCC\">");
			foreach (var descLine in description)
				writer.WriteLine("\t\t" + descLine + "<br><br>");
			writer.WriteLine();
		}

		public static void WriteTable(StreamWriter writer, TableCell[] cells)
		{
			writer.WriteLine("\t\t<table>");

			WriteCell(writer, TableCell.Head, true);
			foreach (var cell in cells)
				WriteCell(writer, cell);

			writer.WriteLine("\t\t</table>");
		}

		static void WriteCell(StreamWriter writer, TableCell cell, bool head = false)
		{
			var style = "td";
			if (head)
				style = "th style=\"background-color: #DDDDDD;\" ";

			writer.WriteLine();
			writer.WriteLine("\t\t\t<tr>");

			writer.WriteLine("\t\t\t\t<" + style + ">" + cell.Name + "</td>");

			writer.WriteLine("\t\t\t\t<" + style + ">" + cell.Type + "</td>");

			var desc = "";
			foreach (var desc1 in cell.Desc)
				desc += desc1 + "<br>";
			writer.WriteLine("\t\t\t\t<" + style + ">" + desc + "</td>");

			writer.WriteLine("\t\t\t</tr>");
		}

		public static void WriteEnd(StreamWriter writer)
		{
			writer.WriteLine("\t<p>Warrior's Snuggery Rules Document. Generated for " + Settings.Version + " at " + DateTime.Now + ".</p>");
			writer.WriteLine("\t</body>");
			writer.WriteLine("</html>");
		}
	}

	static class ObjectWriter
	{
		public static void WriteActors(StreamWriter writer)
		{
			var infos = Assembly.Load("WarriorsSnuggery").GetTypes().Where(t => t.Name.EndsWith("Info") && t.Namespace == "WarriorsSnuggery.Objects.Parts" && !t.IsAbstract);

			bool first = true;
			foreach (var info in infos)
			{
				var attrib = info.GetCustomAttribute(typeof(DescAttribute));
				HTMLWriter.WriteRuleHead(writer, info.Name.Replace("PartInfo", ""), attrib == null ? new string[] { "No Description." } : ((DescAttribute)attrib).Desc);

				WriteWithType(writer, info);

				Console.Write((first ? "" : ", ") + info.Name.Replace("PartInfo", ""));
				if (first)
					first = false;
			}
			Console.WriteLine();
		}

		public static void WriteParticles(StreamWriter writer)
		{
			var info = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Objects.ParticleType");

			WriteWithType(writer, info);
		}

		public static void WriteTerrain(StreamWriter writer)
		{
			var info = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Objects.TerrainType");

			WriteWithType(writer, info);
		}

		public static void WriteWalls(StreamWriter writer)
		{
			var info = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Objects.WallType");

			WriteWithType(writer, info);
		}

		public static void WriteWeapons(StreamWriter writer)
		{
			var info = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Objects.WeaponType");

			WriteWithType(writer, info);
		}

		public static void WriteMaps(StreamWriter writer)
		{
			var info = Assembly.Load("WarriorsSnuggery").GetType("WarriorsSnuggery.Maps.MapInfo");

			WriteWithType(writer, info);
		}

		public static void WriteMapGenerators(StreamWriter writer)
		{
			var infos = Assembly.Load("WarriorsSnuggery").GetTypes().Where(t => t.Name.EndsWith("Info") && t.Namespace == "WarriorsSnuggery.Maps" && !t.IsAbstract);

			bool first = true;
			foreach (var info in infos)
			{
				if (info.Name == "MapInfo")
					continue;

				var attrib = info.GetCustomAttribute(typeof(DescAttribute));
				HTMLWriter.WriteRuleHead(writer, info.Name.Replace("Info", ""), attrib == null ? new string[] { "No Description." } : ((DescAttribute)attrib).Desc);

				WriteWithType(writer, info);

				Console.Write((first ? "" : ", ") + info.Name.Replace("Info", ""));
				first = false;
			}
			Console.WriteLine();
		}

		static void WriteWithType(StreamWriter writer, Type info)
		{
			var variables = info.GetFields().Where(f => f.IsInitOnly && f.GetCustomAttribute(typeof(DescAttribute)) != null).ToArray();
			var cells = new TableCell[variables.Length];
			for (int i = 0; i < variables.Length; i++)
			{
				var variable = variables[i];
				var name = variable.Name;
				var type = getNameOfType(variable.FieldType.Name);
				var desc = ((DescAttribute)variable.GetCustomAttribute(typeof(DescAttribute))).Desc;
				cells[i] = new TableCell(name, type, desc);
			}
			HTMLWriter.WriteTable(writer, cells);
		}

		static string getNameOfType(string name)
		{
			name = name.Replace("Single", "Float");
			name = name.Replace("Int32", "Integer");

			return name;
		}
	}
}
