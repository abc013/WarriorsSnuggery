using System;
using System.IO;

namespace WarriorsSnuggery.Docs
{
	public static class DocumentWriter
	{
		static StreamWriter writer;

		public static void BeginDocument()
		{
			writer = new StreamWriter(FileExplorer.MainDirectory + "Documentation.html");

			writer.WriteLine("<html>");
			writer.WriteLine("<head>");

			writer.WriteLine($"<title>{Program.Title}</title>");

			writer.WriteLine(
				"<style>" +
				"body { line-height: 0.6; }" +
				"table { margin: 10px; box-shadow: 1px 0px 10px " + DocumentationUtils.Colors[0] + "; width: 70%; border-collapse: collapse; }\n" +
				"td { border: 1px solid " + DocumentationUtils.Colors[1] + "; padding: 8px; }\n" +
				"th { border: 1px solid " + DocumentationUtils.Colors[1] + "; padding: 8px; }\n" +
				"tr:nth-child(even) { background-color: " + DocumentationUtils.Colors[0] + "; }\n" +
				"h1 { margin-bottom: 0px; }\n" +
				"h2 { margin-bottom: 0px; }\n" +
				"h3 { margin-bottom: 0px; }\n" +
				"img { filter: drop-shadow(0px 10px 5px #999); image-rendering: pixelated; }" +
				"</style>");

			writer.WriteLine("</head>");
			writer.WriteLine("<body>");
			writer.WriteLine("<img src=\"core/contents/graphics/UI/logo.png\" width=100%/>");
		}

		public static void WriteIndex()
		{
			writer.WriteLine(DocumentationUtils.Header("Table of Contents", 1, false));
			writer.WriteLine(IndexWriter.GetResult());
			writer.Flush();
		}

		public static void WriteChapter(ChapterWriter chapter)
		{
			writer.WriteLine(chapter.GetResult());
			writer.Flush();
		}

		public static void EndDocument()
		{
			writer.WriteLine("<hr>");
			writer.WriteLine($"<p>{Program.Title}. Generated for {Settings.Version} at {DateTime.Now}.</p>");
			writer.WriteLine("</body>");
			writer.WriteLine("</html>");

			writer.Flush();
			writer.Close();
		}
	}
}
