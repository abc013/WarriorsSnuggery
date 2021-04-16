using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery
{
	public static class Settings
	{
		public const byte MaxTeams = 8;

		public const string Version = "(Release) 2.5";

		public const int UpdatesPerSecond = 60;

		public static int BatchSize = 4096;

		public static int MaxSheets = 4;

		public static int SheetSize = 1024;
		public static float SheetHalfPixel => 0.1f / SheetSize;

		public static int VisbilityMargin = 3072;

		public static int FrameLimiter = 0;

		public static float ScrollSpeed = 6;

		public static int EdgeScrolling = 4;

		public static bool EnableCheats = false;

		public static bool DeveloperMode = false;

		public static bool EnableInfoScreen;

		public static bool Fullscreen = true;

		public static int Width = 1920;

		public static int Height = 1080;

		public static bool PartyMode = false;

		public static bool VSync = true;

		public static bool EnablePixeling = false;

		public static bool EnableTextShadowing = true;

		public static bool FirstStarted = true;

		public static float MasterVolume = 1f;

		public static float EffectsVolume = 1f;

		public static float MusicVolume = 0.5f;

		public static Dictionary<string, Keys> KeyDictionary = new Dictionary<string, Keys>();

		public static int CurrentMap = -1;

		public static Keys GetKey(string value)
		{
			if (!KeyDictionary.ContainsKey(value))
				throw new InvalidNodeException(string.Format("Unable to find keyboard key with name {0}.", value));

			return KeyDictionary[value];
		}

		public static void Initialize(bool newSettings)
		{
			if (!newSettings && FileExplorer.Exists(FileExplorer.MainDirectory, "Settings.yaml"))
				load();
			else
				defaultKeys();

			// Set FirstStarted to 0.
			if (FirstStarted)
				Save();
		}

		static void load()
		{
			var fields = TypeLoader.GetFields(typeof(Settings), false);

			var nodes = TextNodeLoader.FromFile(FileExplorer.MainDirectory, "Settings.yaml");
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Keys":
						foreach (var key in node.Children)
							KeyDictionary.Add(key.Key, KeyInput.ToKey(key.Value));
						break;
					default:
						// null is used for static classes.
						TypeLoader.SetValue(null, fields, node);

						break;
				}
			}
		}

		static void defaultKeys()
		{
			KeyDictionary.Clear();
			KeyDictionary.Add("Pause", Keys.P);
			KeyDictionary.Add("CameraLock", Keys.L);
			KeyDictionary.Add("MoveUp", Keys.W);
			KeyDictionary.Add("MoveDown", Keys.S);
			KeyDictionary.Add("MoveLeft", Keys.A);
			KeyDictionary.Add("MoveRight", Keys.D);
			KeyDictionary.Add("MoveAbove", Keys.E);
			KeyDictionary.Add("MoveBelow", Keys.R);
			KeyDictionary.Add("CameraUp", Keys.Up);
			KeyDictionary.Add("CameraDown", Keys.Down);
			KeyDictionary.Add("CameraLeft", Keys.Left);
			KeyDictionary.Add("CameraRight", Keys.Right);
		}

		public static void Save()
		{
			using var writer = new System.IO.StreamWriter(FileExplorer.MainDirectory + "Settings.yaml");

			writer.WriteLine("BatchSize=" + BatchSize);
			writer.WriteLine("MaxSheets=" + MaxSheets);
			writer.WriteLine("SheetSize=" + SheetSize);
			writer.WriteLine("FrameLimiter=" + FrameLimiter);
			writer.WriteLine("ScrollSpeed=" + ScrollSpeed);
			writer.WriteLine("EdgeScrolling=" + EdgeScrolling);
			writer.WriteLine("EnableCheats=" + EnableCheats);
			writer.WriteLine("DeveloperMode=" + DeveloperMode);
			writer.WriteLine("Fullscreen=" + Fullscreen);
			writer.WriteLine("Width=" + Width);
			writer.WriteLine("Height=" + Height);
			writer.WriteLine("VSync=" + VSync);
			writer.WriteLine("EnablePixeling=" + EnablePixeling);
			writer.WriteLine("EnableTextShadowing=" + EnableTextShadowing);
			writer.WriteLine("FirstStarted=" + false);
			writer.WriteLine("MasterVolume=" + MasterVolume);
			writer.WriteLine("EffectsVolume=" + EffectsVolume);
			writer.WriteLine("MusicVolume=" + MusicVolume);

			writer.WriteLine("Keys=");
			foreach (var key in KeyDictionary)
				writer.WriteLine("\t" + key.Key + "=" + key.Value);

			writer.Flush();
			writer.Close();
		}
	}
}
