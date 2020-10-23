using System;
using System.Linq;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public class WaveController
	{
		readonly int waves;
		int currentWave;

		readonly Game game;

		readonly MapGeneratorInfo[] generators;
		readonly MapLoader loader;
		int countdown;

		public WaveController(Game game)
		{
			this.game = game;

			waves = Math.Min((int)Math.Ceiling(Math.Sqrt((game.Statistics.Difficulty / 2 + 1) * game.Statistics.Level)), 10) + 1;
			generators = game.MapType.GeneratorInfos.Where(g => g is PatrolGeneratorInfo info && info.UseForWaves).ToArray();
			loader = new MapLoader(game.World, game.World.Map);

			if (game.MapType.FromSave && game.Statistics.Waves > 0)
				currentWave = game.Statistics.Waves;

			if (generators.Length == 0)
				throw new InvalidNodeException("The GameMode WAVES can not be executed because there are no available PatrolGenerators for it.");
		}

		public void Tick()
		{
			if (--countdown < 0)
			{
				if (game.World.ActorLayer.NonNeutralActors.Any(a => a.Team != Actor.PlayerTeam && a.WorldPart != null && a.WorldPart.KillForVictory))
					countdown = Settings.UpdatesPerSecond * 10;
			}
			else if (countdown == 0)
				CreateNextWave();
			else
			{
				var seconds = countdown / Settings.UpdatesPerSecond + 1;
				if ((countdown + 1) % Settings.UpdatesPerSecond == 0)
				{
					if (currentWave == waves)
						game.AddInfoMessage(200, Color.Yellow + "Transfer in " + seconds + " second" + (seconds > 1 ? "s" : string.Empty));
					else
						game.AddInfoMessage(200, Color.White + "Wave " + (currentWave + 1) + " in " + (seconds % 2 == 0 ? Color.White : Color.Red) + seconds + " second" + (seconds > 1 ? "s" : string.Empty));
				}
			}
		}

		public void CreateNextWave()
		{
			if (++currentWave > waves)
				return;

			game.AddInfoMessage(200, ((currentWave == waves) ? Color.Green : Color.White) + "Wave " + currentWave + "/" + waves);
			if (game.ScreenControl.FocusedType == UI.ScreenType.DEFAULT && !game.Editor)
				((UI.DefaultScreen)game.ScreenControl.Focused).SetWave(currentWave, waves);
			var generatorInfo = (PatrolGeneratorInfo)generators[game.SharedRandom.Next(generators.Length)];
			var generator = new PatrolGenerator(game.SharedRandom, loader, generatorInfo);

			generator.Generate();

			var actors = game.World.ActorLayer.NonNeutralActors.FindAll(a => a.Team != Actor.PlayerTeam && a.IsBot);

			foreach (var actor in actors)
				actor.BotPart.Target = new Objects.Weapons.Target(game.World.LocalPlayer);
		}

		public int CurrentWave()
		{
			return currentWave;
		}

		public bool Done()
		{
			return currentWave > waves;
		}
	}
}
