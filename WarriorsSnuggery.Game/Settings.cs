using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.IO;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery
{
	public static class Settings
	{
		public const string Version = "(Release) 2.7";

		public const int UpdatesPerSecond = 60;

		// Modifier to adjust down if lagging occurs. Zero disables thread sleeping.
		[DefaultValue(0f)]
		public static float ThreadSleepFactor;

		[DefaultValue(false)]
		public static bool IgnoreRequiredAttribute;

		[DefaultValue(false)]
		public static bool LogTimeMeasuring;

		[DefaultValue(4096)]
		public static int BatchSize;

		[DefaultValue(4)]
		public static int MaxSheets;

		[DefaultValue(1024)]
		public static int SheetSize;
		public static float SheetHalfPixel => 0.1f / SheetSize;

		[DefaultValue(2048)]
		public static int VisibilityMargin;

		[DefaultValue(true)]
		public static bool LoadSoft;

		[DefaultValue(0)]
		public static int FrameLimiter;

		[DefaultValue(6)]
		public static float ScrollSpeed;

		[DefaultValue(4)]
		public static int EdgeScrolling;

		[DefaultValue(false)]
		public static bool EnableCheats;

		[DefaultValue(false)]
		public static bool DeveloperMode;

		[DefaultValue(false)]
		public static bool EnableInfoScreen;

		[DefaultValue(true)]
		public static bool Fullscreen;

		[DefaultValue(1920)]
		public static int Width;

		[DefaultValue(1080)]
		public static int Height;

		[DefaultValue(false)]
		public static bool PartyMode;

		[DefaultValue(true)]
		public static bool VSync;

		[DefaultValue(false)]
		public static bool EnablePixeling;

		[DefaultValue(true)]
		public static bool EnableTextShadowing;

		[DefaultValue(true)]
		public static bool EnableWeatherEffects;

		[DefaultValue(true)]
		public static bool FirstStarted;

		[DefaultValue(1f)]
		public static float MasterVolume;

		[DefaultValue(1f)]
		public static float EffectsVolume;

		[DefaultValue(0.5f)]
		public static float MusicVolume;

		[DefaultValue(true)]
		public static bool InvertMouseScroll;

		[DefaultValue(-1)]
		public static int CurrentMap;

		[DefaultValue(true)]
		public static bool LockCameraToPlayer;

		[DefaultValue("Being")]
		public static string Name;

		public static readonly Dictionary<string, Keys> KeyDictionary = new Dictionary<string, Keys>();

		public static readonly List<string> PackageList = new List<string>();

		public static Keys GetKey(string value)
		{
			if (!KeyDictionary.ContainsKey(value))
				throw new NullReferenceException($"Unable to find keyboard key with name {value}.");

			return KeyDictionary[value];
		}

		public static void Initialize(bool newSettings)
		{
			var fields = TypeLoader.GetFields(typeof(Settings), false);
			foreach (var field in fields)
			{
				if (field.IsLiteral || field.IsInitOnly)
					continue;

				var attributes = field.GetCustomAttributes(typeof(DefaultValueAttribute), false);
				if (attributes.Length != 0)
				{
					var attribute = (DefaultValueAttribute)attributes[0];
					field.SetValue(null, attribute.Default);
				}
			}

			if (!newSettings && File.Exists(FileExplorer.MainDirectory + "Settings.yaml"))
				load();

			if (KeyDictionary.Count == 0)
				defaultKeys();

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
							KeyDictionary.Add(key.Key, TextNodeConverter.Convert<Keys>(key));
						break;
					case "Packages":
						foreach (var key in node.Children)
							PackageList.Add(key.Key);
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
			KeyDictionary.Add("Activate", Keys.Space);
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
			// HACK: while saving, set FirstStarted to false.
			var firstStarted = FirstStarted;
			FirstStarted = false;

			using var writer = new System.IO.StreamWriter(FileExplorer.MainDirectory + "Settings.yaml");

			var fields = TypeLoader.GetFields(typeof(Settings), false);
			foreach(var field in fields)
			{
				if (field.IsLiteral || field.IsInitOnly)
					continue;

				var fieldValue = field.GetValue(null);

				var attributes = field.GetCustomAttributes(typeof(DefaultValueAttribute), false);
				if (attributes.Length != 0)
				{
					var attribute = (DefaultValueAttribute)attributes[0];
					if (fieldValue.Equals(attribute.Default))
						continue;
				}

				writer.WriteLine($"{field.Name}={fieldValue}");
			}

			writer.WriteLine("Packages=");
			foreach (var package in PackageList)
				writer.WriteLine($"\t{package}=");

			writer.WriteLine("Keys=");
			foreach (var key in KeyDictionary)
				writer.WriteLine($"\t{key.Key}={key.Value}");

			writer.Flush();
			writer.Close();

			FirstStarted = firstStarted;
		}
	}
}
