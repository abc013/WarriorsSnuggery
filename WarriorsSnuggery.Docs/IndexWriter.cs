using System;
using System.IO;

namespace WarriorsSnuggery.Docs
{
	public static class IndexWriter
	{
		static readonly MemoryStream stream = new MemoryStream();
		static readonly StreamWriter writer = new StreamWriter(stream);

		public static void WriteIndex()
		{
			var types = Enum.GetValues<DocumentationType>();

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
			writer.Flush();
		}

		public static string GetResult()
		{
			stream.Seek(0, SeekOrigin.Begin);
			return new StreamReader(stream).ReadToEnd();
		}
	}
}
