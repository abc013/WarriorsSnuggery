using System;
using System.Diagnostics;
using OpenTK.Graphics.ES30;

namespace WarriorsSnuggery
{
	class Program
	{
		public static Random SharedRandom = new Random();
		public static bool isDebug;

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
					run();
				}
				catch (Exception e)
				{
					window.Visible = false;
					Log.WriteExeption(e);
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Ouch! An error occurred. Damn.");
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("For more details, check the logs (Press 'o' on Windows). Please report the files to abc013 (See authors.html for contact).");
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
			if (!FileExplorer.CheckDll())
			{
				Console.WriteLine("\"OpenTK.dll\" has not been found.");
				Console.ReadKey(true);
				return;
			}

			Settings.DeveloperMode = isDebug;
			Settings.Initialize();
			if (Settings.FirstStarted)
				firstStarted();
			window = new Window();

			if (!checkGLVersion())
			{
				Console.WriteLine("Shader Language Version (OpenGL version) is under 3.00.");
				Console.WriteLine("Please try to run the program with a graphics card that supports this Version.");
				Console.WriteLine("Press 'y' to start the game anyways or press any key to leave.");
				var info = Console.ReadKey(true).KeyChar;
				if (info != 'y')
					return;
			}

			window.Run(60, Settings.FrameLimiter);
		}

		static void firstStarted()
		{
			using (var writer = new System.IO.StreamWriter(FileExplorer.MainDirectory + "WS.yaml"))
			{
				writer.WriteLine("FrameLimiter=" + Settings.FrameLimiter);
				writer.WriteLine("ScrollSpeed=" + Settings.ScrollSpeed);
				writer.WriteLine("EdgeScrolling=" + Settings.EdgeScrolling);
				writer.WriteLine("DeveloperMode=" + Settings.DeveloperMode.GetHashCode());
				writer.WriteLine("Fullscreen=" + Settings.Fullscreen.GetHashCode());
				writer.WriteLine("Width=" + Settings.Width);
				writer.WriteLine("Height=" + Settings.Height);
				writer.WriteLine("AntiAliasing=" + Settings.AntiAliasing.GetHashCode());
				writer.WriteLine("EnablePixeling=" + Settings.EnablePixeling.GetHashCode());
				writer.WriteLine("FirstStarted=" + 0);

				writer.WriteLine("Keys=");
				foreach (var key in Settings.KeyDictionary)
				{
					writer.WriteLine("\t" + key.Key + "=" + key.Value);
				}

				writer.Flush();
				writer.Close();
			}
		}

		static bool checkGLVersion()
		{
			var version = float.Parse(GL.GetString(StringName.ShadingLanguageVersion).Substring(0,4));

			return version >= 300;
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