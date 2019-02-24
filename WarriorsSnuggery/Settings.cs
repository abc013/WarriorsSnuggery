using System;
using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public static class Settings
	{
		public const string Version = "(Playtest) 1.3";

		public static int FrameLimiter;

		// 1: very slow 2: slow 3: normal 4: high 5: legendary high
		public static float ScrollSpeed;

		public static bool DeveloperMode;

		public static bool EnableDebug { get { return DeveloperMode; } private set { } }

		public static bool Fullscreen;

		public static int Width;

		public static int Height;

		public static bool PartyMode;

		public static bool AntiAliasing;

		public static bool EnablePixeling;

		public static bool FirstStarted;

		public static int EdgeScrolling;

		public static Dictionary<string, string> KeyDictionary = new Dictionary<string, string>();

		public static void Initialize()
		{
			foreach(var node in RuleReader.Read(FileExplorer.FindIn(FileExplorer.MainDirectory, "WS", ".yaml")))
			{
				switch(node.Key)
				{
					case "FrameLimiter":
						FrameLimiter = node.ToInt();
						if (FrameLimiter == 0) FrameLimiter = (int) OpenTK.DisplayDevice.Default.RefreshRate;
						break;
					case "ScrollSpeed":
						ScrollSpeed = node.ToFloat();
						break;
					case "EdgeScrolling":
						EdgeScrolling = node.ToInt();
						break;
					case "DeveloperMode":
						DeveloperMode = node.ToBoolean();
						break;
					case "Fullscreen":
						Fullscreen = node.ToBoolean();
						break;
					case "Width":
						Width = node.ToInt();
						break;
					case "Height":
						Height = node.ToInt();
						break;
					case "AntiAliasing":
						AntiAliasing = node.ToBoolean();
						break;
					case "EnablePixeling":
						EnablePixeling = node.ToBoolean();
						break;
					case "FirstStarted":
						FirstStarted = node.ToBoolean();
						break;
					case "Keys":
						foreach(var key in node.Children)
						{
							KeyDictionary.Add(key.Key, key.Value);
						}
						break;
				}
			}
		}

		public static string Key(string value)
		{
			try
			{
				return KeyDictionary[value];
			}
			catch(Exception e)
			{
				throw new YamlInvalidNodeException(string.Format("Unable to find key {0}.", value), e);
			}
		}
	}
}
