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
		public const int SheetSize = 512;

		public const int MaxTeams = 8;

		public const string Version = "(Playtest) 1.5";

		public const int UpdatesPerSecond = 60;

		public static int FrameLimiter = 0;

		public static float ScrollSpeed = 6;

		public static int EdgeScrolling = 4;

		public static bool DeveloperMode = false;

		public static bool EnableDebug { get { return DeveloperMode; } private set { } }

		public static bool EnableInfoScreen;

		public static bool Fullscreen = true;

		public static int Width = 1920;

		public static int Height = 1080;

		public static bool PartyMode = false;

		public static bool AntiAliasing = false;

		public static bool EnablePixeling = false;

		public static bool EnableTextShadowing = true;

		public static bool FirstStarted = true;

		public static float MasterVolume = 1f;

		public static float EffectsVolume = 1f;

		public static float MusicVolume = 1f;

		public static Dictionary<string, string> KeyDictionary = new Dictionary<string, string>();

		public static string Key(string value)
		{
			if (!KeyDictionary.ContainsKey(value))
				throw new YamlInvalidNodeException(string.Format("Unable to find keyboard key with name {0}.", value));

			return KeyDictionary[value];
		}

		public static void Initialize()
		{
			FrameLimiter = (int)OpenTK.DisplayDevice.Default.RefreshRate;

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
					case "MasterVolume":
						MasterVolume = node.Convert<float>();
						break;
					case "EffectsVolume":
						EffectsVolume = node.Convert<float>();
						break;
					case "MusicVolume":
						MusicVolume = node.Convert<float>();
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
				writer.WriteLine("MasterVolume=" + MasterVolume);
				writer.WriteLine("EffectsVolume=" + EffectsVolume);
				writer.WriteLine("MusicVolume=" + MusicVolume);

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
