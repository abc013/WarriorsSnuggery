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

		readonly MapGeneratorInfo[] generators;
		readonly MapLoader loader;

		bool awaitingNextWave;
		int countdown;

		public WaveController(Game game)
		{
			this.game = game;

			waves = Math.Min((int)Math.Ceiling(Math.Sqrt((game.Statistics.Difficulty / 2 + 1) * game.Statistics.Level)), 10) + 1;
			generators = game.MapType.GeneratorInfos.Where(g => g is PatrolGeneratorInfo info && info.UseForWaves).ToArray();
			loader = new MapLoader(game.World, game.World.Map);

			if (game.MapType.FromSave && game.Statistics.Waves > 0)
				CurrentWave = game.Statistics.Waves;

			if (generators.Length == 0)
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

			var generatorInfo = (PatrolGeneratorInfo)generators[game.SharedRandom.Next(generators.Length)];
			var generator = new PatrolGenerator(game.SharedRandom, loader, generatorInfo);

			generator.Generate();

			var actors = game.World.ActorLayer.ToAdd().Where(a => a.Team != Actor.PlayerTeam && a.IsBot);

			foreach (var actor in actors)
				actor.BotPart.Target = new Objects.Weapons.Target(game.World.LocalPlayer);

			waveActors = actors.Where(a => a.WorldPart != null && a.WorldPart.KillForVictory).ToList();
		}
	}
}
