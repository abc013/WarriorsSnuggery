using System;
using System.Collections.Generic;
using System.IO;

namespace WarriorsSnuggery.Docs
{
	public static class HTMLWriter
	{
		public static readonly string[] Colors = new[]
		{
			"#EEEEEE",
			"#DDDDDD",
			"#CCCCCC"
		};

		static StreamWriter writer;

		public static void BeginDocument(StreamWriter writer)
		{
			HTMLWriter.writer = writer;

			writer.WriteLine("<html>");
			writer.WriteLine("<head>");

			writer.WriteLine($"<title>{Program.Title}</title>");

			writer.WriteLine(
				"<style>" +
				"table { margin: 10px; box-shadow: 1px 0px 10px " + Colors[0] + "; width: 70%; border-collapse: collapse; }\n" +
				"td { border: 1px solid " + Colors[1] + "; padding: 8px; }\n" +
				"th { border: 1px solid " + Colors[1] + "; padding: 8px; }\n" +
				"tr:nth-child(even) { background-color: " + Colors[0] + "; }\n" +
				"h1 { margin-bottom: 0px; }\n" +
				"h2 { margin-bottom: 0px; }\n" +
				"h3 { margin-bottom: 0px; }\n" +
				"img { filter: drop-shadow(0px 10px 5px #999); image-rendering: pixelated; }" +
				"</style>");

			writer.WriteLine("\t</head>");
			writer.WriteLine("\t<body>");
			writer.WriteLine("<img src=\"core/contents/graphics/UI/logo.png\" width=100%/>");
		}

		public static void WriteIndex()
		{
			WriteHeader("Table of Contents", 1);
			IndexWriter.WriteIndex();
			writer.WriteLine(IndexWriter.GetResult());
		}

		public static void WriteDoc(DocumentationType type, int number)
		{
			var name = type.GetName();

			WriteHeader($"{number}. {name}", 1, number);

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
				writer.WriteLine($"\t\t{descLine}<br><br>");
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
				style = $"th style=\"background-color:{Colors[1]};\" ";

			writer.WriteLine();
			writer.WriteLine("\t\t\t<tr>");

			writer.WriteLine($"\t\t\t\t<{style}>{cell.Name}</td>");

			writer.WriteLine($"\t\t\t\t<{style}>{cell.Type}</td>");

			var desc = "";
			foreach (var desc1 in cell.Desc)
				desc += desc1 + "<br>";
			writer.WriteLine($"\t\t\t\t<{style}>{desc}</td>");

			if (showValues)
				writer.WriteLine($"\t\t\t\t<{style}>{cell.Value}</td>");

			writer.WriteLine("\t\t\t</tr>");
		}

		public static void EndDocument()
		{
			writer.WriteLine($"\t<p>{Program.Title}. Generated for {Settings.Version} at {DateTime.Now}.</p>");
			writer.WriteLine("\t</body>");
			writer.WriteLine("</html>");
		}
	}
}
