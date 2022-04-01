using WarriorsSnuggery;
using WarriorsSnuggery.Scripting;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.UI.Objects;
using WarriorsSnuggery.Objects.Weapons;

namespace Mission
{
	public class Tutorial4Script : MissionScriptBase
	{
		bool speedSpellActivated, damageSpellActivated, playerswitched;
		Actor target;

		public Tutorial4Script(string file, Game game) : base(file, game)
		{
			OnStart = onStart;
			game.Stats.Lifes = 1;
		}

		void onStart()
		{
			game.Stats.KeyFound = false;

			world.ShroudLayer.RevealShroudRectangular(Actor.PlayerTeam, CPos.Zero, new CPos(18 * 1024, 5 * 1024, 0), true);

			void message2() => game.ScreenControl.ShowMessage(new Message(message3, new[]
			{
				$"Hover above the spells on the right.",
				$"This way, you can see all the information.",
				$"As you can see, some of the spells are {Color.Grey}still locked{Color.White}.",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));

			void message3()
			{
				Tick += tickSpeedSpell;
				game.ScreenControl.ShowMessage(new Message(() => { }, new[]
				{
					$"You can rotate through them with your mouse wheel.",
					$"You can activate the selected one using right click.",
					$"Now, let's try one of them!",
					$"{Color.Cyan}Activate the {Color.Magenta}BOOST SPELL{Color.White}!"
				}));
			}

			game.ScreenControl.ShowMessage(new Message(message2, new[]
			{
				$"Welcome to {Color.Red}Stage 4{Color.White}!",
				$"Let's look at using {Color.Yellow}Spells{Color.White} first!",
				$"The spell bar is located on the right side.",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));
		}

		void tickSpeedSpell()
		{
			if (!speedSpellActivated && world.LocalPlayer.EffectActive(WarriorsSnuggery.Spells.EffectType.SPEED))
			{
				speedSpellActivated = true;
				game.ScreenControl.ShowMessage(new Message(() => { }, new[]
				{
					$"Nicely done! Now, you can run faster!",
					$"After some time, however, the effect is used up.",
					$"It reloads some time and then you can reuse it.",
					$"{Color.Cyan}Move to the right{Color.White} to continue!"
				}));
			}

			if (!speedSpellActivated || world.LocalPlayer.Position.X < 17 * 1024)
				return;

			Tick -= tickSpeedSpell;
			Tick += tickDamageSpell;

			world.ShroudLayer.RevealShroudRectangular(Actor.PlayerTeam, new CPos(18 * 1024, 0, 0), new CPos(24 * 1024, 16 * 1024, 0), true);

			for (int i = 1; i < 5; i++)
				world.WallLayer.Remove(new MPos(18 * 2, i));

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"Okay, now we are going to attack a target.",
				$"For that, we need to use a fitting spell.",
				$"This would probably be one that {Color.Yellow}increases damage{Color.White}.",
				$"{Color.Cyan}Activate a fitting {Color.Magenta}spell{Color.White} to continue!"
			}));

			world.Add(target = ActorCache.Create(world, ActorCache.Types["dummy_hard"], new CPos(20 * 1024 + 512, 13 * 1024 + 512, 0), 1));
		}

		void tickDamageSpell()
		{
			if (!damageSpellActivated)
			{
				if (world.LocalPlayer.EffectActive(WarriorsSnuggery.Spells.EffectType.DAMAGE))
				{
					damageSpellActivated = true;
					game.ScreenControl.ShowMessage(new Message(() => { }, new[]
					{
						$"Good! With that, you can attack the target now!",
						$"Like the speed effect, this one also vanishes.",
						$"It reloads some time and then it can be reused.",
						$"{Color.Cyan}Kill the target{Color.White} to continue!"
					}));
				}

				if (!target.IsAlive)
					world.Add(target = ActorCache.Create(world, ActorCache.Types["dummy_hard"], new CPos(20 * 1024 + 512, 13 * 1024 + 512, 0), 1));
			}

			if (!damageSpellActivated || target.IsAlive)
				return;

			Tick -= tickDamageSpell;
			Tick += tickPreKill;

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"Well done! That's it with spells for now.",
				$"Let's get to lifes: They save you from death.",
				$"The hearts in the top left corner show your life count.",
				$"{Color.Cyan}Move down{Color.White} to continue!"
			}));
		}

		void tickPreKill()
		{
			if (world.LocalPlayer.Position.Y < 17 * 1024)
				return;

			world.ShroudLayer.RevealShroudRectangular(Actor.PlayerTeam, new CPos(7 * 1024, 18 * 1024, 0), new CPos(24 * 1024, 24 * 1024, 0), true);

			for (int i = 1; i < 5; i++)
				world.WallLayer.Remove(new MPos((18 + i) * 2 + 1, 18));

			Tick -= tickPreKill;

			game.ScreenControl.ShowMessage(new Message(() => { Tick += tickKill; }, new[]
			{
				$"Okay, so to show you how this works,",
				$"we're going to kill you.",
				$"Just keep an eye on the hearts in the top left.",
				$"Press {Color.Cyan}continue{Color.White} to get yourself killed!"
			}));
		}

		void tickKill()
		{
			target.Position = new CPos(game.SharedRandom.Next(7 * 1024, 24 * 1024), game.SharedRandom.Next(17 * 1024, 24 * 1024), 0);
			var weapon = WeaponCache.Create(world, WeaponCache.Types["ninja_star"], new Target(world.LocalPlayer), target);
			weapon.Height = game.SharedRandom.Next(50, 250);
			world.Add(weapon);

			if (game.Stats.Lifes == 0)
			{
				Tick -= tickKill;
				Tick += tickChangeCreature;

				game.ScreenControl.ShowMessage(new Message(() => { }, new[]
				{
					$"There we go! And see, you are not dead (yet)!",
					$"Reviving also doesn't count to deaths stats.",
					$"You can refill your lifes using a life pad.",
					$"{Color.Cyan}Move down{Color.White} to continue!"
				}));
			}
		}

		void tickChangeCreature()
		{
			if (world.LocalPlayer.Position.X >= 7 * 1024 || world.LocalPlayer.Position.Y < 6 * 1024)
				return;

			Tick -= tickChangeCreature;

			world.ShroudLayer.RevealShroudRectangular(Actor.PlayerTeam, new CPos(0, 6 * 1024, 0), new CPos(5 * 1024, 17 * 1024, 0), true);

			for (int i = 1; i < 5; i++)
				world.WallLayer.Remove(new MPos(i * 2 + 1, 7));

			void message2() => game.ScreenControl.ShowMessage(new Message(message3, new[]
			{
				$"Hover above the creatures on the left.",
				$"This way, you can see all the information.",
				$"As you can see, some are {Color.Grey}still locked{Color.White}.",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));

			void message3() => game.ScreenControl.ShowMessage(new Message(() => { Tick += tickEnd; }, new[]
			{
				$"You can select one by using shift+mouse wheel.",
				$"Then, you can switch to it with shift+right click.",
				$"Now, let's try one of them!",
				$"{Color.Cyan}Switch to the {Color.Green}Slime actor{Color.White}!"
			}));

			game.ScreenControl.ShowMessage(new Message(message2, new[]
			{
				$"Finally, you can switch creatures.",
				$"For that, look at the bar on the left",
				$"It contains all sorts of creatures.",
				$"Press {Color.Cyan}continue{Color.White} to proceed!"
			}));
		}

		void tickEnd()
		{
			if (!playerswitched)
            {
				if (world.LocalPlayer.Type == ActorCache.Types["slime_big_playable"])
				{
					playerswitched = true;

					game.ScreenControl.ShowMessage(new Message(() => { }, new[]
					{
						$"You did it! Now, you are ready.",
						$"You almost finished the tutorial!",
						$"Finally, {Color.Cyan}Kill the target {Color.White}below!",
						$"Good luck in your future missions!"
					}));

					world.Add(target = ActorCache.Create(world, ActorCache.Types["dummy_hard"], new CPos(2 * 1024 + 512, 15 * 1024 + 512, 0), 1));
					world.Add(ActorCache.Create(world, ActorCache.Types["mainmenu_pad"], world.Map.BottomRightCorner / new CPos(2, 2, 2) - new CPos(256, 256, 0)));
				}
				else
					return;
			}

			if (target.IsAlive)
				return;

			Tick -= tickEnd;

			for (int i = 1; i < 5; i++)
				world.WallLayer.Remove(new MPos(6 * 2, 9 + i));

			game.Stats.KeyFound = true;
		}
	}
}