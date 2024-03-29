﻿using System;
using System.IO;

namespace WarriorsSnuggery
{
	public static class Log
	{
		static TextWriter exceptionWriter;
		static TextWriter debugWriter;

		public static bool Initialized;

		public static void InitLogs()
		{
			Directory.CreateDirectory(FileExplorer.Logs);

			try
			{
				exceptionWriter = new StreamWriter(File.Create(FileExplorer.Logs + "exception.log"));
				Console.SetError(exceptionWriter);

				debugWriter = new StreamWriter(File.Create(FileExplorer.Logs + "debug.log"));
			}
			catch
			{
				Console.WriteLine("Unable to create logs. Using console instead.");
				exceptionWriter = Console.Out;
				debugWriter = Console.Out;
			}

			Initialized = true;
		}

		public static void Exeption(object exception)
		{
			exceptionWriter.WriteLine(exception);
			exceptionWriter.Flush();

			writeDebug("ERRO", "An error occurred. Check 'exception.log' for details.");
		}

		public static void Performance(double ms, string text)
		{
			writeDebug("PERF", $"({ms:0000.0}ms)->{text}");
		}

		public static void LoaderDebug(string loader, string text)
		{
			writeDebug("LOAD", $"({loader}) {text}");
		}

		public static void LoaderWarning(string loader, string text)
		{
			writeDebug("WARN", $"({loader}) {text}");
		}

		public static void Debug(string text)
		{
			writeDebug("INFO", text);
		}

		public static void Warning(string text)
		{
			writeDebug("WARN", text);
		}

		static void writeDebug(string prefix, string text)
		{
			debugWriter.WriteLine($"[{DateTime.Now:T}] {prefix}:\t{text}");
			debugWriter.Flush();
		}

		public static void Close()
		{
			exceptionWriter.Dispose();
			debugWriter.Dispose();
		}
	}
}
