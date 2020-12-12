using WarriorsSnuggery;
using WarriorsSnuggery.Scripting;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Particles;
using System.Linq;
using System;

namespace Mission
{
	public class TutorialScript : MissionScriptBase
	{
		bool collectablesTriggered = false;
		bool enemiesKilledTriggered = false;
		int triggercountdown = 50;

		Actor[] keys;
		Actor[] targets;
		readonly Random random;

		public TutorialScript(string file, Game game) : base(file, game)
		{
			random = Program.SharedRandom;
		}

		public override void OnStart()
		{
			game.AddInfoMessage(200, "Tutorial script started");

			targets = game.World.ActorLayer.Actors.Where(a => a.Type == ActorCreator.Types["dummy"]).ToArray();
			keys = game.World.ActorLayer.Actors.Where(a => a.Type == ActorCreator.Types["key"]).ToArray();
		}

		public override void Tick()
		{
			// Collect the keys
			if (!collectablesTriggered)
			{
				var collectables = true;
				foreach (var key in keys)
				{
					if (!key.Disposed)
					{
						collectables = false;
						break;
					}
				}

				if (collectables)
				{
					collectablesTriggered = true;

					game.World.WallLayer.Remove(new MPos(20 * 2, 6));
					game.World.WallLayer.Remove(new MPos(20 * 2, 7));
					game.World.WallLayer.Remove(new MPos(20 * 2, 8));
					game.World.WallLayer.Remove(new MPos(20 * 2, 9));

					for (int i = 0; i < 40; i++)
					{
						var x = 20 * 1024 - 512;
						var y = 6 * 1024 - 512 + i * 128;

						var init = new ParticleInit(ParticleCreator.Types["beam"], new CPos(x, y, 0), 0);
						game.World.Add(new Particle(world, init));
					}
				}
			}

			// Kill the enemies
			if (!enemiesKilledTriggered)
			{
				var enemiesKilled = true;
				foreach (var target in targets)
				{
					if (target.IsAlive)
					{
						enemiesKilled = false;
						break;
					}
				}

				if (enemiesKilled)
					enemiesKilledTriggered = true;
			}
			else if (triggercountdown-- == 0)
			{
				spawnMoney();
				spawnActor("tut_5", new CPos(1024 * 36, 1024 * 8 - 512, 0));
				spawnActor("tech_pad", new CPos(1024 * 42 + 512, 1024 * 6 - 512, 0));
				spawnActor("actors_pad", new CPos(1024 * 42 + 512, 1024 * 10 - 512, 0));
				spawnActor("tut_6", new CPos(1024 * 45, 1024 * 8 - 512, 0));
				spawnActor("mainmenu_pad", new CPos(1024 * 46 + 512, 1024 * 8 - 512, 0));
			}
		}

		void spawnActor(string type, CPos position)
		{
			for (int i = 0; i < 20; i++)
			{
				var x = random.Next(1024) - 512 + position.X;
				var y = random.Next(1024) - 512 + position.Y;

				var init = new ParticleInit(ParticleCreator.Types["beam"], new CPos(x, y, 0), random.Next(1024));
				game.World.Add(new Particle(world, init));
			}

			game.World.Add(ActorCreator.Create(game.World, type, position));
		}

		void spawnMoney()
		{
			string[] types =
			{
				"gold",
				"silver",
				"silver",
				"silver"
			};

			for (int i = 0; i < 800; i++)
			{
				var x1 = random.Next(4096 - 512) + 38 * 1024 - 256;
				var y1 = random.Next(1024 * 6) + 256;

				var init = new ParticleInit(ParticleCreator.Types["beam"], new CPos(x1, y1, 0), random.Next(1024));
				game.World.Add(new Particle(world, init));

				game.World.Add(ActorCreator.Create(game.World, types[random.Next(types.Length)], new CPos(x1, y1, 0)));
			}

			for (int i = 0; i < 800; i++)
			{
				var x2 = random.Next(4096 - 512) + 38 * 1024 - 256;
				var y2 = random.Next(1024 * 6) + 1024 * 9 - 256;

				var init = new ParticleInit(ParticleCreator.Types["beam"], new CPos(x2, y2, 0), random.Next(1024));
				game.World.Add(new Particle(world, init));

				game.World.Add(ActorCreator.Create(game.World, types[random.Next(types.Length)], new CPos(x2, y2, 0)));
			}
		}
	}
}