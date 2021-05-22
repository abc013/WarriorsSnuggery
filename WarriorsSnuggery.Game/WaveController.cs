using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery
{
	public class WaveController
	{
		public readonly int Waves;
		public int CurrentWave { get; private set; }
		public bool Done { get; private set; }

		List<Actor> waveActors;

		readonly Game game;
		readonly Random random;

		readonly PatrolPlacerInfo[] placers;

		bool awaitingNextWave;
		int countdown;

		public WaveController(Game game)
		{
			this.game = game;
			random = new Random(game.Seed);

			Waves = (int)Math.Ceiling(MathF.Sqrt((game.Save.Difficulty / 2 + 1) * game.Save.Level));
			Waves = Math.Clamp(Waves, 1, 4);

			placers = game.MapType.PatrolPlacers.Where(p => p.UseForWaves).ToArray();

			if (game.MapType.IsSave && game.Save.Waves > 0)
				CurrentWave = game.Save.Waves;

			if (placers.Length == 0)
				throw new InvalidNodeException("The GameMode WAVES can not be executed because there are no available PatrolGenerators for it.");

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

			if (awaitingNextWave)
			{
				if (countdown <= 0)
				{
					nextWave();
					awaitingNextWave = false;
				}
				else if (countdown % Settings.UpdatesPerSecond == 0)
				{
					var seconds = countdown / Settings.UpdatesPerSecond;
					game.AddInfoMessage(200, Color.White + "Wave " + (CurrentWave + 1) + " in " + (seconds % 2 == 0 ? Color.White : Color.Red) + seconds + " second" + (seconds > 1 ? "s" : string.Empty));
				}

				countdown--;
			}
		}

		public void AwaitNextWave()
		{
			if (CurrentWave >= Waves)
			{
				Done = true;
				return;
			}

			awaitingNextWave = true;
			// Give 10 seconds
			countdown = Settings.UpdatesPerSecond * 10;
		}

		void nextWave()
		{
			if (++CurrentWave > Waves)
				return;

			game.AddInfoMessage(200, ((CurrentWave == Waves) ? Color.Green : Color.White) + "Wave " + CurrentWave + "/" + Waves);

			var placerInfo = placers[random.Next(placers.Length)];
			var placer = new PatrolPlacer(game.SharedRandom, game.World, placerInfo);

			var actors = placer.PlacePatrols();
			foreach (var actor in actors)
				actor.Bot.Target = new Objects.Weapons.Target(game.World.LocalPlayer);

			waveActors = actors.Where(a => a.WorldPart != null && a.WorldPart.KillForVictory).ToList();
		}
	}
}
