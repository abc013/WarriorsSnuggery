using System.Diagnostics;

namespace WarriorsSnuggery
{
	public sealed class Timer : Stopwatch
	{
		public static new Timer StartNew()
		{
			var timer = new Timer();
			timer.Start();

			return timer;
		}

		public void StopAndWrite(string text)
		{
			Stop();
			Log.Performance(Elapsed.TotalMilliseconds, text);
		}

		public double StopAndGetMilliseconds()
		{
			Stop();
			return Elapsed.TotalMilliseconds;
		}
	}
}
