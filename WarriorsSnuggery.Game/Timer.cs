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
			Log.Performance(watch.Elapsed.TotalMilliseconds, text);
		}

		public double Stop()
		{
			watch.Stop();
			return watch.Elapsed.TotalMilliseconds;
		}

		public void Restart()
		{
			watch.Restart();
		}
	}
}
