using System;
using System.IO;

namespace WarriorsSnuggery
{
	public static class Log
	{
		static TextWriter exceptionWriter;
		static TextWriter performanceWriter;
		static TextWriter debugWriter;

		public static bool Initialized;

		public static void InitLogs()
		{
			Directory.CreateDirectory(FileExplorer.Logs);

			exceptionWriter = new StreamWriter(FileExplorer.CreateFile(FileExplorer.Logs, "exception", ".log"));
			Console.SetError(exceptionWriter);

			performanceWriter = new StreamWriter(FileExplorer.CreateFile(FileExplorer.Logs, "performance", ".log"));
			debugWriter = new StreamWriter(FileExplorer.CreateFile(FileExplorer.Logs, "debug", ".log"));

			Initialized = true;
		}

		public static void WriteExeption(object exception)
		{
			exceptionWriter.WriteLine(exception);
			exceptionWriter.Flush();
		}

		public static void WritePerformance(long ms, string text)
		{
			performanceWriter.WriteLine(ms + "ms\t\t " + text);
			performanceWriter.Flush();
		}

		public static int DebugIndentation;
		public static void WriteDebug(string text)
		{
			debugWriter.WriteLine(new string('\t', DebugIndentation) + text);
			debugWriter.Flush();
		}

		public static void Close()
		{
			exceptionWriter.Dispose();
			performanceWriter.Dispose();
			debugWriter.Dispose();
		}
	}
}
