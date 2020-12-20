using System;
using System.Collections.Generic;
using System.IO;
using WarriorsSnuggery;

namespace DocWriter
{
	public static class HTMLWriter
	{
		public static readonly string[] LightModeColors = new[]
		{
			"#EEEEEE",
			"#DDDDDD",
			"#CCCCCC"
		};
		public static readonly string[] DarkModeColors = new[]
		{
			"#111111",
			"#222222",
			"#333333"
		};
		public static string[] Colors;

		static StreamWriter writer;

		public static void SetWriter(StreamWriter writer)
		{
			HTMLWriter.writer = writer;
		}

		public static void WriteHead()
		{
			writer.WriteLine("<html>");
			writer.WriteLine("\t<head>");

			writer.WriteLine($"\t\t<title>{Program.Title}</title>");

			writer.WriteLine("\t\t<style>");
			if (Program.DarkMode)
			{
				writer.WriteLine("body { background-color: #000000; color: #BBBBBB; }");
				writer.WriteLine("table { margin: 10px; box-shadow: 1px 0px 10px " + Colors[0] + "; width: 70%; border-collapse: collapse; }");
				writer.WriteLine("td { border: 1px solid " + Colors[2] + "; padding: 8px; }");
				writer.WriteLine("th { border: 1px solid " + Colors[2] + "; padding: 8px; color: #DDDDFF; }");
				writer.WriteLine("tr:nth-child(even) { background-color: " + Colors[0] + "; }");
				writer.WriteLine("tr { background-color: " + Colors[1] + "; color: #DDDDDD; }");
				writer.WriteLine("hr { color: " + Colors[2] + "; }");
				writer.WriteLine("h1 { margin-bottom: 0px; color: #FFFFFF; }");
				writer.WriteLine("h2 { margin-bottom: 0px; color: #FFFFFF; }");
				writer.WriteLine("h3 { margin-bottom: 0px; color: #EEEEEE; }");
			}
			else
			{
				writer.WriteLine("table { margin: 10px; box-shadow: 1px 0px 10px " + Colors[0] + "; width: 70%; border-collapse: collapse; }");
				writer.WriteLine("td { border: 1px solid " + Colors[1] + "; padding: 8px; }");
				writer.WriteLine("th { border: 1px solid " + Colors[1] + "; padding: 8px; }");
				writer.WriteLine("tr:nth-child(even) { background-color: " + Colors[0] + "; }");
				writer.WriteLine("h1 { margin-bottom: 0px; }");
				writer.WriteLine("h2 { margin-bottom: 0px; }");
				writer.WriteLine("h3 { margin-bottom: 0px; }");
			}
			writer.WriteLine("\t\t</style>");

			writer.WriteLine("\t</head>");
			writer.WriteLine("\t<body>");
			writer.WriteLine("<img src=\"misc/UI/logo.png\"/>");
		}

		public static void WriteIndex(DocumentationType[] types)
		{
			WriteHeader("Table of Contents", 1);

			writer.WriteLine($"\t\t<ul>");

			int id = 1;
			foreach (var type in types)
			{
				writer.WriteLine($"\t\t<li><h4><a href=\"#{id}\">");
				writer.WriteLine($"\t\t\t{id}. {type.GetName()}");
				writer.WriteLine($"\t\t</a></h4></li>");

				id++;
			}

			writer.WriteLine($"\t\t</ul>");
		}

		public static void WriteDoc(DocumentationType type, int id)
		{
			var name = type.GetName();

			WriteHeader($"{id}. {name}", 1, id);

			typeof(ObjectWriter).GetMethod($"Write{name}").Invoke(null, null);
		}

		public static void WriteHeader(string head, int importance = 2, int id = -1)
		{
			var idString = id > 0 ? $" id=\"{id}\"" : string.Empty;
			writer.WriteLine($"\t\t<h{importance}{idString}> {head} </h{importance}>");
			writer.WriteLine("\t\t<hr>");
		}

		public static void WriteDescription(string[] description)
		{
			foreach (var descLine in description)
				writer.WriteLine("\t\t" + descLine + "<br><br>");
			writer.WriteLine();
		}

		public static void WriteTable(List<TableCell> cells, bool showValues)
		{
			writer.WriteLine("\t\t<table>");

			writeCell(TableCell.Head, showValues, true);
			foreach (var cell in cells)
				writeCell(cell, showValues);

			writer.WriteLine("\t\t</table>");
		}

		static void writeCell(TableCell cell, bool showValues, bool head = false)
		{
			var style = "td";
			if (head)
				style = "th style=\"background-color: " + Colors[1] + ";\" ";

			writer.WriteLine();
			writer.WriteLine("\t\t\t<tr>");

			writer.WriteLine("\t\t\t\t<" + style + ">" + cell.Name + "</td>");

			writer.WriteLine("\t\t\t\t<" + style + ">" + cell.Type + "</td>");

			var desc = "";
			foreach (var desc1 in cell.Desc)
				desc += desc1 + "<br>";
			writer.WriteLine("\t\t\t\t<" + style + ">" + desc + "</td>");

			if (showValues)
				writer.WriteLine("\t\t\t\t<" + style + ">" + cell.Value + "</td>");

			writer.WriteLine("\t\t\t</tr>");
		}

		public static void WriteEnd()
		{
			writer.WriteLine($"\t<p>{Program.Title}. Generated for {Settings.Version} at {DateTime.Now}.</p>");
			writer.WriteLine("\t</body>");
			writer.WriteLine("</html>");
		}
	}
}
