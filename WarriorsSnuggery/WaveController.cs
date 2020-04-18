﻿using System;
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

		public WaveController(Game game)
		{
			this.game = game;

			waves = Math.Min((int)Math.Ceiling(Math.Sqrt((game.Statistics.Difficulty / 2 + 1) * game.Statistics.Level)), 10);
			generators = game.MapType.GeneratorInfos.Where(g => g is PatrolGeneratorInfo && ((PatrolGeneratorInfo)g).UseForWaves).ToArray();

			if (generators.Length == 0)
				throw new YamlInvalidNodeException("The GameMode WAVES can not be executed because there are no available PatrolGenerators for it.");
		}

		public void Tick()
		{
			var actor = game.World.Actors.FirstOrDefault(a => !(a.Team == Actor.PlayerTeam || a.Team == Actor.NeutralTeam) && a.WorldPart != null && a.WorldPart.KillForVictory);

			if (actor == null)
				CreateNextWave();
		}

		public void CreateNextWave()
		{
			if (++currentWave > waves)
				return;

			game.AddInfoMessage(120, "Wave " + currentWave + "/" + waves);
			if (game.ScreenControl.FocusedType == UI.ScreenType.DEFAULT)
				((UI.DefaultScreen)game.ScreenControl.Focused).SetWave(currentWave, waves);
			var generatorInfo = (PatrolGeneratorInfo)generators[game.SharedRandom.Next(generators.Length)];
			var generator = new PatrolGenerator(game.SharedRandom, game.World.Map, game.World, generatorInfo);

			generator.Generate();

			var actors = game.World.getActorsToAdd().FindAll(a => !(a.Team == Actor.PlayerTeam || a.Team == Actor.NeutralTeam));

			foreach (var actor in actors)
			{
				if (actor.IsBot)
					actor.BotPart.Target = new Objects.Weapons.Target(game.World.LocalPlayer);
			}
		}

		public bool Done()
		{
			return currentWave > waves;
		}
	}
}
