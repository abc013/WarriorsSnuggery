using WarriorsSnuggery;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Scripting;
using WarriorsSnuggery.UI.Objects;

namespace Mission
{
	public class Tutorial3Script : MissionScriptBase
	{
		int moneyrows = 22;

		public Tutorial3Script(PackageFile packageFile, Game game) : base(packageFile, game)
		{
			OnStart = onStart;
			Tick += tickHealth;
		}

		void onStart()
		{
			game.Stats.KeyFound = false;

			world.ShroudLayer.RevealShroudCircular(game.World, Actor.PlayerTeam, new CPos(5 * 1024, 1 * 1024, 0), 0, 16, true);
			world.ShroudLayer.RevealShroudCircular(game.World, Actor.PlayerTeam, new CPos(5 * 1024, 10 * 1024, 0), 0, 16, true);
			world.ShroudLayer.RevealShroudCircular(game.World, Actor.PlayerTeam, new CPos(5 * 1024, 20 * 1024, 0), 0, 24, true);
			world.ShroudLayer.RevealShroudCircular(game.World, Actor.PlayerTeam, new CPos(5 * 1024, 28 * 1024, 0), 0, 16, true);
			world.ShroudLayer.RevealShroudCircular(game.World, Actor.PlayerTeam, new CPos(5 * 1024, 34 * 1024, 0), 0, 16, true);

			void message2() => game.ScreenControl.ShowMessage(new Message(message3, new[]
			{
				$"To your left, there are {Color.Yellow}green potions{Color.White}.",
				$"These are poisonous, watch out!",
				$"To your right, there are {Color.Yellow}red potions{Color.White}, which heal you.",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));

			void message3() => game.ScreenControl.ShowMessage(new Message(() => { Tick += tickHealth; }, new[]
			{
				$"If you look down, you can see some {Color.Yellow}apples{Color.White}.",
				$"They also help you regain health. You used them and",
				$"the red potions in the last stage, if you remember!",
				$"Press {Color.Cyan}Continue {Color.White} and {Color.Cyan}Move down {Color.White}to proceed."
			}));

			game.ScreenControl.ShowMessage(new Message(message2, new[]
			{
				$"Welcome to {Color.Red}Stage 3{Color.White}!",
				$"Here you will now get to know {Color.Yellow}Collectables{Color.White}!",
				$"",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));
		}

		void tickHealth()
		{
			if (world.LocalPlayer.Position.Y < 5 * 1024)
				return;

			Tick -= tickHealth;
			Tick += tickMoney;
		}

		void tickMoney()
		{
			if (moneyrows-- < 0)
			{
				game.ScreenControl.ShowMessage(new Message(() => { }, new[]
				{
					$"This is money. You will find it on your missions.",
					$"It is primarily used for buying new things.",
					$"We will look into that further down!",
					$"Move down to {Color.Cyan}continue{Color.White}!"
				}));

				Tick -= tickMoney;
				Tick += tickShop;
			}
			else
			{
				var height = moneyrows * 256 + 6144 + 512;

				var money_types = new[] { "blue", "gold", "gold", "silver", "silver", "silver" };
				for (int i = 0; i < 100; i++)
				{
					var randomPos = () => new CPos(game.SharedRandom.Next(2048, 19200) - 512, height + game.SharedRandom.Next(1024), 0);

					world.Add(ActorCache.Create(world, ActorCache.Types[money_types[game.SharedRandom.Next(money_types.Length)]], randomPos()));
					world.Add(ParticleCache.Create(world, ParticleCache.Types["fire"], randomPos(), 10));
				}
			}
		}

		void tickShop()
		{
			if (world.LocalPlayer.Position.Y < 13 * 1024)
				return;

			Tick -= tickShop;
			Tick += tickPostShop;

			void message2() => game.ScreenControl.ShowMessage(new Message(message3, new[]
			{
				$"Further down to the left, there is the {Color.Yellow}life pad{Color.White}.",
				$"You can buy {Color.Yellow}extra lifes{Color.White} there. And finally, ",
				$"there's the {Color.Yellow}trophy pad{Color.White}, which shows you your {Color.Yellow}trophies{Color.White}.",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));

			void message3() => game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"We will explain in {Color.Red}stage 4 {Color.White}what all the stuff is for!",
				$"",
				$"Now: buy the {Color.Green}SLIME ACTOR{Color.White} and the {Color.Magenta}WARM UP SPELL{Color.White}!",
				$"If you need more money, go and collect some!"
			}));

			game.ScreenControl.ShowMessage(new Message(message2, new[]
			{
				$"You can see four pads here.",
				$"The left top one is for buying {Color.Green}creatures{Color.White}.",
				$"The right top pad is for {Color.Magenta}spells{Color.White}.",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));
		}

		void tickPostShop()
		{
			if (!game.Stats.UnlockedActors.Contains("Slime") || !game.Stats.UnlockedSpells.Contains("Warm_up"))
				return;

			Tick -= tickPostShop;
			Tick += tickMana;

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"Well done!",
				$"",
				$"",
				$"Move further down to {Color.Cyan}continue {Color.White}!"
			}));
		}

		void tickMana()
		{
			if (world.LocalPlayer.Position.Y < 26 * 1024)
				return;

			Tick -= tickMana;
			Tick += tickExit;

			for (int i = 0; i < 3; i++)
				world.Add(ActorCache.Create(world, ActorCache.Types["portion_blue"], new CPos((i + 5) * 1024, 28 * 1024, 0), 30));

			for (int i = 0; i < 300; i++)
			{
				var randomPos = () => new CPos(game.SharedRandom.Next(2 * 1024, 10 * 1024) - 512, game.SharedRandom.Next(27 * 1024, 30 * 1024), 0);

				world.Add(ActorCache.Create(world, ActorCache.Types["mana_splash"], randomPos()));
				world.Add(ParticleCache.Create(world, ParticleCache.Types["mana_splash"], randomPos(), 30));
			}

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"This is {Color.Yellow}mana{Color.White}. It is required to cast spells.",
				$"As mentioned, you will get to know more in {Color.Red}Stage 4{Color.White} about it.",
				$"",
				$"Move further down to {Color.Cyan}continue {Color.White}!"
			}));
		}

		void tickExit()
		{
			if (world.LocalPlayer.Position.Y < 30 * 1024)
				return;

			Tick -= tickExit;

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"Now, finally, there is the {Color.Yellow}key{Color.White}!",
				$"You already now what it is for! Go for it!",
				$"",
				$"Then, exit to {Color.Cyan}continue {Color.White} to {Color.Red}Stage 4{Color.White}!"
			}));

			world.Add(ActorCache.Create(world, ActorCache.Types["key"], new CPos(11 * 512, 69 * 512, 0), 1));

			world.Add(ActorCache.Create(world, ActorCache.Types["tutorial_pad"], new CPos(11 * 512, 67 * 512, 0), 1));
		}
	}
}