/*
 * User: Andreas
 * Date: 06.01.2018
 * 
 */
using System.Diagnostics;

namespace WarriorsSnuggery
{
	public sealed class Timer
	{
		readonly Stopwatch watch;

		Timer()
		{
			watch = Stopwatch.StartNew();
		}

		public static Timer Start()
		{
			return new Timer();
		}

		public void StopAndWrite(string text)
		{
			watch.Stop();
			Log.WritePerformance(watch.ElapsedMilliseconds, text);
		}

		public long Stop()
		{
			watch.Stop();
			return watch.ElapsedMilliseconds;
		}
	}
}
