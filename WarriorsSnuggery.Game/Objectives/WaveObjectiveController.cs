using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Audio.Music;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objectives
{
	public class WaveObjectiveController : ObjectiveController
	{
		public override string MissionString => "Defend your position from incoming waves!";

		public readonly int Waves;
		[Save, DefaultValue(0)]
		public int CurrentWave { get; private set; }
		bool done;

		List<Actor> waveActors;

		readonly PatrolPlacerInfo[] placers;

		[Save("Countdown"), DefaultValue(0)]
		int countdown;

		public WaveObjectiveController(Game game) : base(game)
		{
			Waves = (int)Math.Ceiling(MathF.Sqrt((game.Save.Difficulty / 2 + 1) * game.Save.Level));
			Waves = Math.Clamp(Waves, 1, 4);

			var mapType = game.MapType.IsSave ? game.Save.CurrentMapType : game.MapType;
			placers = mapType.PatrolPlacers.Where(p => p.UseForWaves).ToArray();

			if (placers.Length == 0)
				throw new InvalidOperationException("The GameMode WAVES can not be executed because there are no available PatrolGenerators for it.");
		}

		public override void Load(TextNodeInitializer initializer)
		{
			initializer.SetSaveFields(this);
		}

		public override TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			saver.AddSaveFields(this);

			return saver;
		}

		public override void Tick()
		{
			if (done)
				return;

			if (countdown == 0)
			{
				// In case of a savegame
				waveActors ??= Game.World.ActorLayer.NonNeutralActors.Where(a => a.Team != Actor.PlayerTeam && a.IsBot && a.WorldPart != null && a.WorldPart.KillForVictory).ToList();

				if (waveActors.Any(a => a.IsAlive))
					return;

				awaitNextWave();
			}
			else
			{
				if (countdown % Settings.UpdatesPerSecond == 0)
				{
					var seconds = countdown / Settings.UpdatesPerSecond;
					Game.AddInfoMessage(200, $"{Color.White}Wave {(CurrentWave + 1)} in {(seconds % 2 == 0 ? Color.White : Color.Red)}{seconds} second{(seconds > 1 ? "s" : string.Empty)}");
				}

				countdown--;

				if (countdown <= 0)
					nextWave();
			}
		}

		void awaitNextWave()
		{
			if (CurrentWave >= Waves)
			{
				done = true;
				Game.VictoryConditionsMet();
				return;
			}

			// Give 10 seconds
			countdown = Settings.UpdatesPerSecond * 10;
		}

		void nextWave()
		{
			if (++CurrentWave > Waves)
				return;

			Game.AddInfoMessage(200, $"{((CurrentWave == Waves) ? Color.Green : Color.White)}Wave {CurrentWave}/{Waves}");

			var placerInfo = placers[Random.Next(placers.Length)];
			var placer = new PatrolPlacer(Game.SharedRandom, Game.World, placerInfo);

			var actors = placer.PlacePatrols();
			foreach (var actor in actors)
				actor.Bot.Target = new Objects.Weapons.Target(Game.World.LocalPlayer);

			waveActors = actors.Where(a => a.WorldPart != null && a.WorldPart.KillForVictory).ToList();

			MusicController.FadeIntenseIn(Settings.UpdatesPerSecond * 20);
		}
	}
}
