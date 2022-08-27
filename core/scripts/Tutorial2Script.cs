using System.Linq;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Scripting;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.Scripts.Core
{
	public class Tutorial2Script : MissionScriptBase
	{
		Actor trainer;

		Actor[] targets;
		bool targetsDead => targets != null && !targets.Any(a => a.IsAlive);

		int explosionCountdown = 7 * Settings.UpdatesPerSecond;

		public Tutorial2Script(PackageFile packageFile, Game game) : base(packageFile, game)
		{
			OnStart = onStart;
			Tick += tickAttackTraining1;
		}

		void onStart()
		{
			game.Stats.KeyFound = false;
			world.Add(trainer = ActorCache.Create(world, ActorCache.Types["ninja_trainer"], world.LocalPlayer.Position - new CPos(0, 4096, 0), Actor.PlayerTeam, true));
			trainer.Bot.Target = new Target(world.LocalPlayer.Position - new CPos(0, 1024, 0));

			world.ShroudLayer.RevealShroudRectangular(Actor.PlayerTeam, new CPos(0, 8 * 1024, 0), new CPos(8 * 1024, world.Map.Bounds.Y * 1024, 0), true);
			world.ShroudLayer.RevealShroudCircular(game.World, Actor.PlayerTeam, new CPos(5 * 1024, 5 * 1024, 0), 16, true);
			
			void message2()
			{
				targets = new[] { spawnTarget(new CPos(3584, 33280, 0)) };
				trainer.Bot.Target = new Target(targets[0].Position + new CPos(-1024, 0, 0));

				game.ScreenControl.ShowMessage(new Message(() => { }, new[]
				{
					$"In Front of you, you can see a target.",
					$"",
					$"{Color.Cyan}Kill it{Color.White}!"
				}));
			}

			game.ScreenControl.ShowMessage(new Message(message2, new[]
			{
				$"There you are! Now it's time for the {Color.Red}Second Stage{Color.White}!",
				$"Here, you'll learn how to {Color.Yellow}Attack{Color.White}.",
				$"You can attack by pressing the {Color.Yellow}left mouse button{Color.White}.",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));
		}

		void tickAttackTraining1()
        {
			if (!targetsDead)
				return;

			Tick -= tickAttackTraining1;
			Tick += tickAttackTraining2;

			game.ScreenControl.ShowMessage(new Message(() => { trainer.Bot.Target = new Target(targets[0].Position + new CPos(-1024, -8 * 1024, 0)); }, new[]
			{
				$"Good! You see the {Color.Yellow}arrow{Color.White} that just appeared?",
				$"It {Color.Yellow}points to a random enemy{Color.White}. When you get hurt,",
				$"it disappears, but it comes back after a while.",
				$"{Color.Cyan}Follow {Color.White}me and {Color.Cyan}kill the targets {Color.White}on the way!"
			}));

			targets = new[] { spawnTarget(new CPos(5632, 26112, 0)), spawnTarget(new CPos(1536, 26112, 0)) };
			game.ScreenControl.ShowArrow();
		}

		void tickAttackTraining2()
        {
			if (!targetsDead || world.LocalPlayer.Position.Y > 20 * 1024)
				return;

			Tick -= tickAttackTraining2;
			Tick += tickAttackTraining3;

			game.ScreenControl.ShowMessage(new Message(() => { trainer.Bot.Target = new Target(new CPos(10 * 1024, 3 * 1024, 0)); }, new[]
			{
				$"Well done! Next, we will look at blocking objects.",
				$"Walls and crates can block your projectiles!",
				$"Now, go ahead and {Color.Cyan}kill the next 4 targets {Color.White}!",
				$"Then, {Color.Cyan}move further upward{Color.White}, I'll see you there."
			}));

			targets = new[] { spawnTarget(new CPos(1536, 15872, 0)), spawnTarget(new CPos(5632 + 1024, 11776, 0)), spawnTarget(new CPos(1536, 8704, 0)), spawnTarget(new CPos(4608, 4608, 0)) };
		}

		void tickAttackTraining3()
		{
			if (world.LocalPlayer.Position.X <= 8192)
				return;

			if (!targetsDead)
			{
				game.ScreenControl.ShowMessage(new Message(() => { }, new[]
				{
					$"You are not done yet!",
					$"First, {Color.Cyan}kill the 4 targets {Color.White}behind you!",
					$"Only then we can continue."
				}));

				return;
			}
			Tick -= tickAttackTraining3;
			Tick += tickPreExplosion;

			game.ScreenControl.ShowMessage(new Message(() => { trainer.Bot.Target = new Target(new CPos(17 * 1024, 5 * 1024, 0)); }, new[]
			{
				$"Very good! Time for some real enemies then!",
				$"{Color.Cyan}Follow me{Color.White}! I just need to open the gate.",
				$"It will take just a few seconds!"
			}));

			world.ShroudLayer.RevealShroudCircular(game.World, Actor.PlayerTeam, new CPos(26 * 1024, 3 * 1024, 0), 20, true);
		}

		void tickPreExplosion()
        {
			if (explosionCountdown-- != 0)
				return;

			game.ScreenControl.ShowMessage(new Message(() => { Tick -= tickPreExplosion; Tick += tickExplosion; }, new[]
			{
				$"Okay, there we go. Gate should open any time soon."
			}));
		}

		void tickExplosion()
		{
			Tick -= tickExplosion;
			Tick += tickPostExplosion;

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"Ouch! Damn. I can't move any further, it hurts!",
				$"At least we regenerate some health over time.",
				$"There are some healing potions, apples and a healing",
				$"pad on the right! {Color.Cyan}Use them to gain health{Color.White}!"
			}));

			var random = game.SharedRandom;

			for (int i = 0; i < 2; i++)
			{
				var weapon = WeaponCache.Create(world, "cannon", new Target(new CPos(random.Next(18 * 512, 22 * 512), random.Next(9 * 512, 10 * 512), 0) * 2), targets[0]);
				world.Add(weapon);

				weapon.Detonate(weapon.Target);
			}

			for (int i = 0; i < 600; i++)
			{
				var particle = ParticleCache.Create(world, "fire", new CPos(random.Next(16 * 512, 26 * 512), random.Next(4 * 512, 9 * 512), 64) * 2);
				world.Add(particle);
			}

			world.LocalPlayer.Health.HP = 100;

			MusicController.FadeIntenseIn(60 * Settings.UpdatesPerSecond);
		}

		void tickPostExplosion()
        {
			if (world.LocalPlayer.Health.HP != world.LocalPlayer.Health.MaxHP && world.ActorLayer.Actors.Any(a => a.Type == ActorCache.Types["apple"] || a.Type == ActorCache.Types["portion_red"]))
				return;

			Tick += () => world.Add(ParticleCache.Create(world, ParticleCache.Types["blood"], trainer.Position + new CPos(0, 0, Program.SharedRandom.Next(10, 100))));

			Tick -= tickPostExplosion;
			Tick += tickFight1;

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"You look way better now!",
				$"I can't move, so you must move on your own.",
				$"{Color.Cyan}Go down and find the exit{Color.White}, but watch out:",
				$"There are some enemies! Good luck!"
			}));

			game.Stats.KeyFound = true;
		}

		void tickFight1()
        {
			if (world.LocalPlayer.Position.X <= 8192 || world.LocalPlayer.Position.Y <= 12 * 1024)
				return;

			Tick -= tickFight1;
			Tick += tickFight2;

			var actor1Positions = new[] { new CPos(18 * 1024, 15 * 1024, 0), new CPos(21 * 1024, 15 * 1024, 0) };

			foreach (var position in actor1Positions)
			{
				var actor = ActorCache.Create(world, ActorCache.Types["slime_medium"], position, 1, true);
				actor.Bot.Target = new Target(world.LocalPlayer);
				world.Add(actor);
			}
		}

		void tickFight2()
        {
			if (world.LocalPlayer.Position.X <= 8192 || world.LocalPlayer.Position.Y <= 2 * 8192)
				return;

			Tick -= tickFight2;
			Tick += tickFight3;

			var actor2Positions = new[] { new CPos(19 * 1024, 19 * 1024, 0), new CPos(12 * 1024, 23 * 1024, 0), new CPos(8 * 1024, 19 * 1024, 0) };

			foreach (var position in actor2Positions)
			{
				var actor = ActorCache.Create(world, ActorCache.Types["slime_medium"], position, 1, true);
				actor.Bot.Target = new Target(world.LocalPlayer);
				world.Add(actor);
			}

			var actorBig = ActorCache.Create(world, ActorCache.Types["slime_big"], new CPos(19 * 1024, 23 * 1024, 0), 1, true);
			actorBig.Bot.Target = new Target(world.LocalPlayer);
			world.Add(actorBig);
		}

		void tickFight3()
        {
			if (world.LocalPlayer.Position.X > 7120 && world.LocalPlayer.Position.Y >= 8192)
				return;

			Tick -= tickFight3;

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"You are going the wrong way!",
				$"Turn around, the exit is somewhere below."
			}));
		}

		Actor spawnTarget(CPos position)
		{
			var actor = ActorCache.Create(world, ActorCache.Types["dummy"], position, 1);
			world.Add(actor);

			return actor;
		}
	}
}