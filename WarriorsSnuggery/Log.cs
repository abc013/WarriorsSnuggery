using System;
using System.IO;

namespace WarriorsSnuggery
{
	public static class Log
	{
		public static TextWriter Exeption;
		public static TextWriter Performance;
		public static TextWriter Debug;

		static bool isClosed;

		public static void InitLogs()
		{
			Directory.CreateDirectory(FileExplorer.Logs);

			Exeption = new StreamWriter(FileExplorer.CreateFile(FileExplorer.Logs, "exception", ".log"));
			Performance = new StreamWriter(FileExplorer.CreateFile(FileExplorer.Logs, "performance", ".log"));
			Debug = new StreamWriter(FileExplorer.CreateFile(FileExplorer.Logs, "debug", ".log"));
		}

		public static void WriteExeption(Exception exception)
		{
			if (isClosed)
				return;

			Exeption.WriteLine(exception.Message);
			Exeption.WriteLine(exception);
			Exeption.Flush();
		}

		public static void WritePerformance(long ms, string text)
		{
			if (isClosed)
				return;

			Performance.WriteLine(ms + "ms\t\t " + text);
			Performance.Flush();
		}

		public static void WriteDebug(string text)
		{
			if (isClosed)
				return;

			Debug.WriteLine(text);
			Debug.Flush();
		}

		public static void Close()
		{
			isClosed = true;
			Exeption.Dispose();
			Performance.Dispose();
			Debug.Dispose();
		}
	}
}
