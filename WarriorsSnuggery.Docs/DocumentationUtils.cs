using System.Collections.Generic;
using System.Text;

namespace WarriorsSnuggery.Docs
{
	public static class DocumentationUtils
	{
		public static readonly string[] Colors = new[]
		{
			"#EEEEEE",
			"#DDDDDD",
			"#CCCCCC"
		};

		public static string Header(string head, int importance, bool includeInIndex = true)
		{
			var builder = new StringBuilder();

			if (includeInIndex)
				IndexWriter.WriteEntry(head, importance);

			builder.AppendLine($"<h{importance} id=\"{head}\"> {head} </h{importance}>");
			builder.AppendLine("<hr>");

			return builder.ToString();
		}

		public static string Description(string[] description)
		{
			var builder = new StringBuilder();

			foreach (var descLine in description)
				builder.AppendLine($"{descLine}<br><br>");
			builder.AppendLine();

			return builder.ToString();
		}

		public static string Table(List<TableCell> cells, bool showValues)
		{
			var builder = new StringBuilder();

			builder.AppendLine("<table>");

			writeCell(builder, TableCell.Head, showValues, true);
			foreach (var cell in cells)
				writeCell(builder, cell, showValues);

			builder.AppendLine("</table>");

			return builder.ToString();
		}

		static void writeCell(StringBuilder builder, TableCell cell, bool showValues, bool head = false)
		{
			var style = "td";
			if (head)
				style = $"th style=\"background-color:{Colors[1]};\" ";

			builder.AppendLine();
			builder.AppendLine("<tr>");

			builder.AppendLine($"<{style}>{cell.Name}</td>");

			builder.AppendLine($"<{style}>{cell.Type}</td>");

			var desc = string.Join("<br>", cell.Desc);
			builder.AppendLine($"<{style}>{desc}</td>");

			if (showValues)
				builder.AppendLine($"<{style}>{cell.Value}</td>");

			builder.AppendLine("</tr>");
		}
	}
}
