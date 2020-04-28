using OpenToolkit.Graphics.OpenGL;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using System;
using System.Diagnostics;

namespace WarriorsSnuggery
{
	class Program
	{
		public const string Title = "Warrior's Snuggery";

		public static Random SharedRandom = new Random();
		public static bool isDebug;
		public static bool NoFullscreen;
		static bool noGLErrors;

		static Window window;

		public static void Main(string[] args)
		{
			isDebug = Debugger.IsAttached;

			FileExplorer.InitPaths();
			Log.InitLogs();
			Console.SetError(Log.Exeption);

			if (!isDebug)
			{
				try
				{
					run(args);
				}
				catch (Exception e)
				{
					window.IsVisible = false;
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
				run(args);
			}

			Log.Close();
		}

		static void run(string[] args)
		{
			var newSettings = false;
			foreach (var arg in args)
			{
				if (arg == "-no-fullscreen")
					NoFullscreen = true;
				else if (arg == "-new-settings")
					newSettings = true;
				else if (arg == "-no-GL-errors")
					noGLErrors = true;
			}

			Settings.Initialize(newSettings);

			var settings1 = new GameWindowSettings
			{
				RenderFrequency = Settings.FrameLimiter,
				UpdateFrequency = Settings.UpdatesPerSecond
			};
			var settings2 = new NativeWindowSettings
			{
				Title = Title,
				API = ContextAPI.OpenGL,
				APIVersion = new Version(3, 2),
				Size = new OpenToolkit.Mathematics.Vector2i(Settings.Width, Settings.Height)
			};
			window = new Window(settings1, settings2);

			if (GL.GetInteger(GetPName.MajorVersion) < 3 && GL.GetInteger(GetPName.MinorVersion) < 2)
			{
				Console.WriteLine("OpenGL version is under 3.20.");
				Console.WriteLine("Please run the program with a graphics card that supports at least OpenGL 3.20.");
				Console.WriteLine("Press 'y' to start the game anyway (will probably crash) or press any key to exit.");
				var info = Console.ReadKey(true).KeyChar;
				if (info != 'y')
					return;
			}
			window.Run();
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
			if (noGLErrors)
				return;

			var error = GL.GetError();

			if (error != ErrorCode.NoError)
				throw new Exception("GraphicError in '" + code + "' :" + error);
		}
	}
}