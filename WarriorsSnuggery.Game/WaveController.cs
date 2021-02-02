using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public class WaveController
	{
		readonly int waves;
		public int CurrentWave { get; private set; }
		public bool Done => CurrentWave > waves;

		List<Actor> waveActors;

		readonly Game game;

		readonly PatrolPlacerInfo[] placers;

		bool awaitingNextWave;
		int countdown;

		public WaveController(Game game)
		{
			this.game = game;

			waves = Math.Min((int)Math.Ceiling(Math.Sqrt((game.Statistics.Difficulty / 2 + 1) * game.Statistics.Level)), 10) + 1;
			placers = game.MapType.PatrolPlacers.Where(p => p.UseForWaves).ToArray();

			if (game.MapType.IsSave && game.Statistics.Waves > 0)
				CurrentWave = game.Statistics.Waves;

			if (placers.Length == 0)
				throw new InvalidTextNodeException("The GameMode WAVES can not be executed because there are no available PatrolGenerators for it.");

			AwaitNextWave();
		}

		public void Tick()
		{
			if (Done)
				return;

			if (!awaitingNextWave)
			{
				// In case of a savegame
				if (waveActors == null)
					waveActors = game.World.ActorLayer.Actors.Where(a => a.Team != Actor.PlayerTeam && a.IsBot && a.WorldPart != null && a.WorldPart.KillForVictory).ToList();

				if (waveActors.Any(a => a.IsAlive))
					return;

				AwaitNextWave();
			}

			if (awaitingNextWave && countdown-- < 0)
			{
				nextWave();
				awaitingNextWave = false;
			}
			else
			{
				var seconds = countdown / Settings.UpdatesPerSecond + 1;
				if ((countdown + 1) % Settings.UpdatesPerSecond == 0)
				{
					if (CurrentWave == waves)
						game.AddInfoMessage(200, Color.Yellow + "Transfer in " + seconds + " second" + (seconds > 1 ? "s" : string.Empty));
					else
						game.AddInfoMessage(200, Color.White + "Wave " + (CurrentWave + 1) + " in " + (seconds % 2 == 0 ? Color.White : Color.Red) + seconds + " second" + (seconds > 1 ? "s" : string.Empty));
				}
			}
		}

		public void AwaitNextWave()
		{
			awaitingNextWave = true;
			// Give 10 seconds
			countdown = Settings.UpdatesPerSecond * 10;
		}

		void nextWave()
		{
			if (++CurrentWave > waves)
				return;

			game.AddInfoMessage(200, ((CurrentWave == waves) ? Color.Green : Color.White) + "Wave " + CurrentWave + "/" + waves);
			game.ScreenControl.UpdateWave(CurrentWave, waves);

			var placerInfo = placers[game.SharedRandom.Next(placers.Length)];
			var placer = new PatrolPlacer(game.SharedRandom, game.World, placerInfo);

			var actors = placer.PlacePatrols();
			foreach (var actor in actors)
				actor.BotPart.Target = new Objects.Weapons.Target(game.World.LocalPlayer);

			waveActors = actors.Where(a => a.WorldPart != null && a.WorldPart.KillForVictory).ToList();
		}
	}
}
