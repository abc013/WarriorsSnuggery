/*
 * User: Andreas
 * Date: 30.09.2017
 * 
 */
using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public static class Settings
	{
		// HACK
		public const bool LoaderWorkAround = false;

		public const int SheetSize = 512;

		public const int MaxTeams = 8;

		public const string Version = "(Playtest) 1.5pre";

		public const int UpdatesPerSecond = 60;

		public static int FrameLimiter;

		public static float ScrollSpeed;

		public static bool DeveloperMode;

		public static bool EnableDebug { get { return DeveloperMode; } private set { } }

		public static bool EnableInfoScreen;

		public static bool Fullscreen;

		public static int Width;

		public static int Height;

		public static bool PartyMode;

		public static bool AntiAliasing;

		public static bool EnablePixeling;

		public static bool EnableTextShadowing;

		public static bool FirstStarted;

		public static int EdgeScrolling;

		public static Dictionary<string, string> KeyDictionary = new Dictionary<string, string>();

		public static string Key(string value)
		{
			if (!KeyDictionary.ContainsKey(value))
				throw new YamlInvalidNodeException(string.Format("Unable to find keyboard key with name {0}.", value));

			return KeyDictionary[value];
		}

		public static void Initialize()
		{
			foreach (var node in RuleReader.Read(FileExplorer.MainDirectory, "WS.yaml"))
			{
				switch (node.Key)
				{
					case "FrameLimiter":
						FrameLimiter = node.Convert<int>();

						if (FrameLimiter == 0 || FrameLimiter > OpenTK.DisplayDevice.Default.RefreshRate)
							FrameLimiter = (int)OpenTK.DisplayDevice.Default.RefreshRate;

						break;
					case "ScrollSpeed":
						ScrollSpeed = node.Convert<float>();
						break;
					case "EdgeScrolling":
						EdgeScrolling = node.Convert<int>();
						break;
					case "DeveloperMode":
						DeveloperMode = node.Convert<bool>();
						break;
					case "Fullscreen":
						Fullscreen = node.Convert<bool>();
						break;
					case "Width":
						Width = node.Convert<int>();
						break;
					case "Height":
						Height = node.Convert<int>();
						break;
					case "AntiAliasing":
						AntiAliasing = node.Convert<bool>();
						break;
					case "EnablePixeling":
						EnablePixeling = node.Convert<bool>();
						break;
					case "EnableTextShadowing":
						EnableTextShadowing = node.Convert<bool>();
						break;
					case "FirstStarted":
						FirstStarted = node.Convert<bool>();
						break;
					case "Keys":
						foreach (var key in node.Children)
						{
							KeyDictionary.Add(key.Key, key.Value);
						}
						break;
				}
			}

			// Set FirstStarted to 0.
			if (FirstStarted)
				Save();
		}

		public static void Save()
		{
			using (var writer = new System.IO.StreamWriter(FileExplorer.MainDirectory + "WS.yaml"))
			{
				writer.WriteLine("FrameLimiter=" + FrameLimiter);
				writer.WriteLine("ScrollSpeed=" + ScrollSpeed);
				writer.WriteLine("EdgeScrolling=" + EdgeScrolling);
				writer.WriteLine("DeveloperMode=" + DeveloperMode.GetHashCode());
				writer.WriteLine("Fullscreen=" + Fullscreen.GetHashCode());
				writer.WriteLine("Width=" + Width);
				writer.WriteLine("Height=" + Height);
				writer.WriteLine("AntiAliasing=" + AntiAliasing.GetHashCode());
				writer.WriteLine("EnablePixeling=" + EnablePixeling.GetHashCode());
				writer.WriteLine("EnableTextShadowing=" + EnableTextShadowing.GetHashCode());
				writer.WriteLine("FirstStarted=" + 0);

				writer.WriteLine("Keys=");
				foreach (var key in KeyDictionary)
				{
					writer.WriteLine("\t" + key.Key + "=" + key.Value);
				}

				writer.Flush();
				writer.Close();
			}
		}
	}
}
