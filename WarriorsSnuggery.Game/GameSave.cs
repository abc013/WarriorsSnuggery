using System.Collections.Generic;
using System.IO;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Maps.Pieces;
using WarriorsSnuggery.Objectives;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery
{
	public sealed class GameSave
	{
		// Saved separately
		public Player Player { get; private set; }
		// Saved separately
		public int GameSaveFormat { get; private set; }
		[Save]
		public string Name { get; private set; }
		public string SaveName { get; private set; }
		public string MapSaveName => SaveName + "_map";

		[Save]
		public string[] ActiveMods { get; private set; } = new string[0];

		// Changing Values
		[Save]
		public int Level { get; private set; }

		[Save]
		public string Actor { get; private set; }
		[Save]
		public float Health { get; private set; }

		// Level Values
		[Save]
		public ObjectiveType CurrentObjective { get; private set; }
		[Save]
		public MissionType CurrentMission { get; private set; }
		// Saved separately
		public MapType CurrentMapType { get; private set; }
		// TODO: save via [Save] attribute
		public Color CurrentAmbience { get; private set; } = Color.White;
		[Save, DefaultValue(0)]
		public int Waves { get; private set; }

		[Save, DefaultValue(false)]
		public bool KeyFound;
		// Saved separately
		public Dictionary<string, bool> CustomConditions { get; private set; } = new Dictionary<string, bool>();

		// Static Values
		[Save]
		public int FinalLevel { get; private set; }
		[Save]
		public int Difficulty { get; private set; }
		[Save]
		public int Seed { get; private set; }
		[Save, DefaultValue(false)]
		public bool Hardcore { get; private set; }

		// Script Values
		[Save, DefaultValue(null)]
		public PackageFile Script { get; private set; }
		// Saved separately
		public TextNode[] ScriptState { get; private set; }
		// Saved separately
		public TextNodeInitializer ObjectiveController { get; private set; } = new TextNodeInitializer(new List<TextNode>());

		GameSave(GameSave save)
		{
			// Copy all fields
			var fields = typeof(GameSave).GetProperties();
			foreach (var properties in fields)
			{
				if (properties.SetMethod != null)
					properties.SetValue(this, properties.GetValue(save));
			}

			// Clone Player
			Player = save.Player.Clone();
		}

		public GameSave Clone()
		{
			return new GameSave(this);
		}

		public GameSave(string filepath) : this()
		{
			SaveName = FileExplorer.FileName(filepath);

			var properties = typeof(GameSave).GetProperties();

			foreach (var node in TextNodeLoader.FromFilepath(filepath))
			{
				switch (node.Key)
				{
					case nameof(Player):
						Player = new Player(new TextNodeInitializer(node.Children));

						break;
					case nameof(CustomConditions):
						foreach (var node2 in node.Children)
							CustomConditions.Add(node2.Key, node2.Convert<bool>());

						break;
					case nameof(CurrentMapType):
						CurrentMapType = MapCache.Types[node.Value];

						break;
					case nameof(ScriptState):
						ScriptState = node.Children.ToArray();

						break;
					case nameof(ObjectiveController):
						ObjectiveController = new TextNodeInitializer(node.Children);

						break;
					default:
						TypeLoader.SetValue(this, properties, node);

						break;
				}
			}
		}

		public GameSave(int difficulty, bool hardcore, string name, int seed) : this()
		{
			GameSaveFormat = Constants.CurrentGameSaveFormat;
			SetName(name);

			ActiveMods = PackageManager.ActivePackages.Select(p => p.InternalName).ToArray();

			Hardcore = hardcore;
			Difficulty = difficulty;
			Seed = seed;

			Level = 1;
			FinalLevel = (difficulty + 1) * 5;

			Actor = GameSaveManager.DefaultSave.Actor;
			Health = GameSaveManager.DefaultSave.Health;

			Player = new Player(2, 0, "Player");
			Player.InitializeWith(this);
		}

		GameSave() { }

		public int CalculateScore()
		{
			// Positive Points
			var score = Level * 100 / FinalLevel;
			score += Player.Kills * 5;
			score += Player.Money * 2;
			score += Player.Lifes * 25;
			// Negative Points
			score -= Player.Deaths * 25;
			return score;
		}

		public void Save(Game game)
		{
			Update(game);

			var saver = new TextNodeSaver();
			saver.AddChildren(nameof(Player), Player.Save());
			saver.Add(nameof(GameSaveFormat), Constants.CurrentGameSaveFormat);
			saver.Add(nameof(CurrentMapType), MapCache.Types[CurrentMapType]);
			saver.Add(nameof(CurrentAmbience), $"{(int)(CurrentAmbience.R * 255)}, {(int)(CurrentAmbience.G * 255)}, {(int)(CurrentAmbience.B * 255)}, {(int)(CurrentAmbience.A * 255)}");

			if (game.Script != null)
			{
				Script = game.Script.PackageFile;
				saver.AddChildren(nameof(ScriptState), game.Script.Save(), true);
			}

			if (game.ObjectiveController != null)
				saver.AddChildren(nameof(ObjectiveController), game.ObjectiveController.Save(), true);

			saver.AddSaveFields(this);

			saver.AddChildren(nameof(CustomConditions), game.ConditionManager.Save(), true);

			using (var writer = new StreamWriter(FileExplorer.Saves + SaveName + ".yaml", false))
			{
				foreach (var savedString in saver.GetStrings())
					writer.WriteLine(savedString);
			}

			PieceSaver.SaveWorld(game.World, FileExplorer.Saves, MapSaveName, true);
		}

		public void Update(Game game, bool levelIncrease = false)
		{
			if (levelIncrease)
				Level++;

			ActiveMods = PackageManager.ActivePackages.Select(p => p.InternalName).ToArray();

			var player = game.World.LocalPlayer;

			Actor = ActorCache.Types[player.Type];

			if (player.IsPlayerSwitch)
				Health = player.GetPart<PlayerSwitchPart>().RelativeHP;
			else
				Health = player.Health == null ? 1 : player.Health.RelativeHP;

			CurrentObjective = game.ObjectiveType;
			CurrentMission = game.MissionType;

			var save = game.Save;
			var mapType = game.MapType;
			CurrentMapType = mapType.IsSave ? save.CurrentMapType : mapType;
			CurrentAmbience = WorldRenderer.Ambient;

			var stats = game.Player;

			KeyFound = !levelIncrease && KeyFound;
			Player = game.Player;
		}

		public void Delete()
		{
			if (File.Exists(FileExplorer.Saves + SaveName + ".yaml"))
				File.Delete(FileExplorer.Saves + SaveName + ".yaml");

			if (File.Exists(FileExplorer.Saves + MapSaveName + ".yaml"))
				File.Delete(FileExplorer.Saves + MapSaveName + ".yaml");
		}

		public bool OutdatedVersion()
		{
			return GameSaveFormat < Constants.CurrentGameSaveFormat;
		}

		public bool InvalidMods()
		{
			// string comparison. Order of mods may be important.
			return string.Join(',', ActiveMods) != string.Join(',', PackageManager.ActivePackages.Select(p => p.InternalName));
		}

		public void SetName(string name)
		{
			Name = name;
			foreach (var c in FileExplorer.InvalidFileChars)
				name = name.Replace(c, '_');

			SaveName = name;
		}
	}
}
