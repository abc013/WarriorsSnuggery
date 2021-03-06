using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using System.Globalization;
using WarriorsSnuggery.Audio;

namespace WarriorsSnuggery
{
	public class Program
	{
		public const string Title = "Warrior's Snuggery";

		public static Random SharedRandom = new Random();

		public static bool IsDebug;
		public static bool NoFullscreen;

		public static bool StartEditor;
		public static bool DisableShroud;
		public static bool IgnoreTech;
		public static string Piece;
		public static string MapType;

		static bool noGLErrors;

		static Window window;

		public static void Main(string[] args)
		{
			IsDebug = Debugger.IsAttached;
			AppDomain.CurrentDomain.UnhandledException += handleError;

			// Use invariant culture because of floats
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

			run(args);

			Log.WriteDebug("Exiting program.");
			Log.Close();
		}

		static void run(string[] args)
		{
			FileExplorer.InitPaths();
			Log.InitLogs();

			var newSettings = false;
			for (int i = 0; i < args.Length; i++)
			{
				var arg = args[i];

				if (arg == "-no-fullscreen")
					NoFullscreen = true;
				else if (arg == "-new-settings")
					newSettings = true;
				else if (arg == "-no-GL-errors")
					noGLErrors = true;
				else if (arg == "-editor")
					StartEditor = true;
				else if (arg == "-no-shroud")
					DisableShroud = true;
				else if (arg == "-ignore-tech")
					IgnoreTech = true;
				else if (arg == "-use-piece")
					Piece = args[++i];
				else if (arg == "-map-type")
					MapType = args[++i];
				else if (arg == "-enable-cheats")
					Settings.EnableCheats = true;
			}

			Settings.Initialize(newSettings);

			var settings1 = new GameWindowSettings
			{
				UpdateFrequency = Settings.UpdatesPerSecond
			};
			if (Settings.FrameLimiter != 0)
				settings1.RenderFrequency = Settings.FrameLimiter;

			var settings2 = new NativeWindowSettings
			{
				Title = Title,
				API = ContextAPI.OpenGL,
				APIVersion = new Version(3, 2),
				Size = new OpenTK.Mathematics.Vector2i(Settings.Width, Settings.Height),
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

		static void handleError(object sender, UnhandledExceptionEventArgs args)
		{
			var e = args.ExceptionObject;
			if (window != null)
				window.IsVisible = false;

			if (Log.Initialized)
				Log.WriteExeption(e);
			else
				Console.WriteLine(e);

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Ouch! An error occurred.");
			Console.ResetColor();
			Console.WriteLine("For more details, check out the logs (press 'o' or open the 'logs' directory).");
			Console.WriteLine("Please report the issue to https://github.com/abc013/WarriorsSnuggery or contact abc013.");
			Console.WriteLine("Thank you!");
			if (!IsDebug)
			{
				var key = Console.ReadKey(true).KeyChar;
				if (key == 'o')
					Process.Start("explorer.exe", "/select, \"" + FileExplorer.Logs + "exception.log\"");
			}
		}

		public static void Exit()
		{
			var watch = Timer.Start();

			GameController.Exit();
			AudioController.Exit();
			Window.CloseWindow();

			Log.WriteDebug("Game closing.");
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