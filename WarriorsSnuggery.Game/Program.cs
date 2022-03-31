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
		public static bool OnlyLoad;

		public static bool StartEditor;
		public static bool DisableShroud;
		public static bool IgnoreTech;
		public static bool DisableScripts;
		public static bool ReloadScripts;
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
		}

		static void run(string[] args)
		{
			FileExplorer.InitPaths();
			Log.InitLogs();
			Log.Debug("Starting program.");

			var newSettings = false;
			var enableCheats = false;
			for (int i = 0; i < args.Length; i++)
			{
				var arg = args[i];

				switch (arg)
				{
					case "-no-fullscreen":
						NoFullscreen = true;
						break;
					case "-new-settings":
						newSettings = true;
						break;
					case "-no-GL-errors":
						noGLErrors = true;
						break;
					case "-editor":
						StartEditor = true;
						break;
					case "-no-shroud":
						DisableShroud = true;
						break;
					case "-ignore-tech":
						IgnoreTech = true;
						break;
					case "-disable-scripts":
						DisableScripts = true;
						break;
					case "-reload-scripts":
						ReloadScripts = true;
						break;
					case "-use-piece":
						Piece = args[++i];
						break;
					case "-map-type":
						MapType = args[++i];
						break;
					case "-enable-cheats":
						enableCheats = true;
						break;
					case "-only-load":
						OnlyLoad = true;
						break;
					default:
						throw new ArgumentException($"Unknown command line argument {arg}.");
				}
			}

			Settings.Initialize(newSettings);
			Settings.EnableCheats |= enableCheats;

			var settings1 = new GameWindowSettings
			{
				UpdateFrequency = Settings.UpdatesPerSecond
			};
			if (Settings.FrameLimiter != 0)
				settings1.RenderFrequency = Settings.FrameLimiter;

			var settings2 = new NativeWindowSettings
			{
				Title = Title,
				//Profile = ContextProfile.Core,
				//Flags = ContextFlags.ForwardCompatible,
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

			Log.Debug("Exiting program.");
			Log.Close();
		}

		static void handleError(object sender, UnhandledExceptionEventArgs args)
		{
			var e = args.ExceptionObject;
			if (window != null)
				window.IsVisible = false;

			if (Log.Initialized)
				Log.Exeption(e);
			else
				Console.WriteLine(e);

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Ouch! An error occurred.");
			Console.ResetColor();
			Console.WriteLine("For more details, please check out the logs (located in 'WarriorsSnuggery/logs/').");
			Console.WriteLine("Please report the issue to https://github.com/abc013/WarriorsSnuggery or contact abc013.");
			Console.WriteLine("Thank you!");
		}

		public static void Exit()
		{
			var watch = Timer.Start();

			GameController.Exit();
			AudioController.Dispose();
			Window.CloseWindow();

			Log.Debug("Game closing.");
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