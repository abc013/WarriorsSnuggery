using System;
using System.Collections.Generic;
using System.Globalization;

namespace WarriorsSnuggery
{
	public static class Settings
	{
		public static readonly IFormatProvider FloatFormat = CultureInfo.InvariantCulture;

		public const int BatchSize = 4096;
		public const int SheetSize = 1024;
		public const float SheetHalfPixel = 0.1f / SheetSize;

		public const int MaxTeams = 8;

		public const string Version = "(Release) 2.1";

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

		public static float MusicVolume = 0.5f;

		public static Dictionary<string, string> KeyDictionary = new Dictionary<string, string>();

		public static string Key(string value)
		{
			if (!KeyDictionary.ContainsKey(value))
				throw new YamlInvalidNodeException(string.Format("Unable to find keyboard key with name {0}.", value));

			return KeyDictionary[value];
		}

		public static void Initialize(bool newSettings)
		{
			FrameLimiter = (int)OpenTK.DisplayDevice.Default.RefreshRate;

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
			foreach (var node in RuleReader.Read(FileExplorer.MainDirectory, "Settings.yaml"))
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
							KeyDictionary.Add(key.Key, key.Value);
						break;
				}
			}
		}

		static void defaultKeys()
		{
			KeyDictionary.Clear();
			KeyDictionary.Add("Pause", "p");
			KeyDictionary.Add("CameraLock", "l");
			KeyDictionary.Add("MoveUp", "w");
			KeyDictionary.Add("MoveDown", "s");
			KeyDictionary.Add("MoveLeft", "a");
			KeyDictionary.Add("MoveRight", "d");
			KeyDictionary.Add("MoveAbove", "e");
			KeyDictionary.Add("MoveBelow", "r");
			KeyDictionary.Add("CameraUp", "up");
			KeyDictionary.Add("CameraDown", "down");
			KeyDictionary.Add("CameraLeft", "left");
			KeyDictionary.Add("CameraRight", "right");
		}

		public static void Save()
		{
			using (var writer = new System.IO.StreamWriter(FileExplorer.MainDirectory + "Settings.yaml"))
			{
				writer.WriteLine("FrameLimiter=" + FrameLimiter);
				writer.WriteLine("ScrollSpeed=" + ScrollSpeed.ToString(FloatFormat));
				writer.WriteLine("EdgeScrolling=" + EdgeScrolling);
				writer.WriteLine("DeveloperMode=" + DeveloperMode.GetHashCode());
				writer.WriteLine("Fullscreen=" + Fullscreen.GetHashCode());
				writer.WriteLine("Width=" + Width);
				writer.WriteLine("Height=" + Height);
				writer.WriteLine("AntiAliasing=" + AntiAliasing.GetHashCode());
				writer.WriteLine("EnablePixeling=" + EnablePixeling.GetHashCode());
				writer.WriteLine("EnableTextShadowing=" + EnableTextShadowing.GetHashCode());
				writer.WriteLine("FirstStarted=" + 0);
				writer.WriteLine("MasterVolume=" + MasterVolume.ToString(FloatFormat));
				writer.WriteLine("EffectsVolume=" + EffectsVolume.ToString(FloatFormat));
				writer.WriteLine("MusicVolume=" + MusicVolume.ToString(FloatFormat));

				writer.WriteLine("Keys=");
				foreach (var key in KeyDictionary)
					writer.WriteLine("\t" + key.Key + "=" + key.Value);

				writer.Flush();
				writer.Close();
			}
		}
	}
}
