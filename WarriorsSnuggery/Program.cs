using OpenTK.Graphics.ES30;
using System;
using System.Diagnostics;

namespace WarriorsSnuggery
{
	class Program
	{
		public static Random SharedRandom = new Random();
		public static bool isDebug;

		static Window window;

		public static void Main()
		{
			isDebug = Debugger.IsAttached;

			FileExplorer.InitPaths();
			Log.InitLogs();
			Console.SetError(Log.Exeption);

			if (!isDebug)
			{
				try
				{
					run();
				}
				catch (Exception e)
				{
					window.Visible = false;
					Log.WriteExeption(e);
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Ouch! An error occurred.");
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("For more details, check the logs (Press 'o' on Windows). Please report the files to abc013 (See authors.html).");
					var key = Console.ReadKey(true).KeyChar;
					if (key == 'o')
					{
						Process.Start("explorer.exe", "/select, \"" + FileExplorer.Logs + "\"");
					}
				}
			}
			else
			{
				run();
			}

			Log.Close();
		}

		static void run()
		{
			if (!FileExplorer.CheckDll(true))
			{
				Console.WriteLine("\"OpenAL32.dll\" has not been found.");
				Console.ReadKey(true);
				return;
			}
			if (!FileExplorer.CheckDll(false))
			{
				Console.WriteLine("\"OpenTK.dll\" has not been found.");
				Console.ReadKey(true);
				return;
			}

			Settings.Initialize();
			window = new Window();

			if (GL.GetInteger(GetPName.MajorVersion) < 3)
			{
				Console.WriteLine("OpenGL version is under 3.00.");
				Console.WriteLine("Please run the program with a graphics card that supports > 3.00.");
				Console.WriteLine("Press 'y' to start the game anyway or press any key to exit.");
				var info = Console.ReadKey(true).KeyChar;
				if (info != 'y')
					return;
			}

			window.Run(Settings.UpdatesPerSecond, Settings.FrameLimiter);
		}

		public static void Exit()
		{
			var watch = Timer.Start();

			GameController.Exit();
			AudioController.Exit();
			Window.CloseWindow();

			watch.StopAndWrite("Disposing");
		}

		[Conditional("DEBUG")]
		[DebuggerStepThrough]
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CheckGraphicsError(string code)
		{
			var error = GL.GetError();

			if (error != ErrorCode.NoError)
			{
				throw new OpenTK.Graphics.GraphicsErrorException("GraphicError in '" + code + "' :" + error);
			}
		}
	}
}