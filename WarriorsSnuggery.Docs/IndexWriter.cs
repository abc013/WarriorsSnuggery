using System.IO;

namespace WarriorsSnuggery.Docs
{
	public static class IndexWriter
	{
		static readonly MemoryStream stream = new MemoryStream();
		static readonly StreamWriter writer = new StreamWriter(stream);

		static int currentImportance = 0;

		public static void BeginIndex()
		{
			writer.WriteLine($"<ul>");
			currentImportance++;
		}

		public static void WriteEntry(string text, int importance)
		{
			while (currentImportance > importance)
				EndIndex();
			while (currentImportance < importance)
				BeginIndex();

			writer.WriteLine($"<li><h4><a href=\"#{text}\">");
			writer.WriteLine($"{text}");
			writer.WriteLine($"</a></h4></li>");
		}

		public static void EndIndex()
		{
			writer.WriteLine($"</ul>");
			currentImportance--;
		}

		public static string GetResult()
		{
			writer.Flush();
			stream.Seek(0, SeekOrigin.Begin);
			return new StreamReader(stream).ReadToEnd();
		}
	}
}
