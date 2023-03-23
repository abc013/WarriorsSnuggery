using System;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Audio.Music;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Scripting;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.Scripts.Core
{
	public class Tutorial1Script : MissionScriptBase
	{
		public Tutorial1Script(PackageFile packageFile, Game game) : base(packageFile, game)
		{
			OnStart += onStart;
			Tick += tickSimpleMovement;
		}

		void onStart()
		{
			world.ShroudLayer.RevealShroudRectangular(Actor.PlayerTeam, new CPos(0, 0, 0), new CPos(12 * 1024, 6 * 1024, 0), true);

			void message2() => game.ScreenControl.ShowMessage(new Message(message3, new[]
			{
				$"In front of you, you can see various crates.",
				$"These don't block your sight, but your movement.",
				$"",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));

			void message3() => game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"Then there are also walls (all around you).",
				$"Some of them block your sight, some don't.",
				$"And most of them block your movement, obviously.",
				$"{Color.Cyan}Move around the Obstacles {Color.White}to proceed."
			}));

			game.ScreenControl.ShowMessage(new Message(message2, new[]
			{
				$"Welcome to Warrior's Snuggery!",
				$"First you'll learn how to {Color.Yellow}Move{Color.White}.",
				$"You can move with the following keys: {Color.Yellow}{Settings.GetKey("MoveUp")}{Color.White},{Color.Yellow}{Settings.GetKey("MoveLeft")}{Color.White},{Color.Yellow}{Settings.GetKey("MoveDown")}{Color.White},{Color.Yellow}{Settings.GetKey("MoveRight")}{Color.White}.",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));
		}

		void tickSimpleMovement()
		{
			if (world.LocalPlayer.TerrainPosition.X <= 16)
				return;

			Tick -= tickSimpleMovement;
			Tick += tickTerrain;

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"Well done!",
				$"In front of you, there are different types of terrain.",
				$"These will change your movement speed.",
				$"Try it out! {Color.Cyan}Move further{Color.White}."
			}));

			world.ShroudLayer.RevealShroudRectangular(Actor.PlayerTeam, new CPos(16 * 1024, 0, 0), new CPos(world.Map.Bounds.X * 1024, 6 * 1024, 0), true);
			world.ShroudLayer.RevealShroudRectangular(Actor.PlayerTeam, new CPos(26 * 1024, 6 * 1024, 0), new CPos(world.Map.Bounds.X * 1024, 7 * 1024, 0), true);
			world.ShroudLayer.RevealShroudRectangular(Actor.PlayerTeam, new CPos(17 * 1024, 7 * 1024, 0), new CPos(world.Map.Bounds.X * 1024, 15 * 1024, 0), true);
		}

		void tickTerrain()
		{
			if (world.LocalPlayer.TerrainPosition.Y <= 6 || world.LocalPlayer.TerrainPosition.X >= 26)
				return;

			Tick -= tickTerrain;
			Tick += tickAir;

			void changePlayer()
			{
				game.ScreenControl.ShowMessage(new Message(() => { }, new[]
				{
					$"There you go!",
					$"",
					$"Once your metamorphose is done,",
					$"{Color.Cyan}Fly {Color.White}over to the other side!"
				}));

				world.BeginPlayerSwitch(ActorCache.Types["shadowrunner_playable"]);
			};

			game.ScreenControl.ShowMessage(new Message(changePlayer, new[]
			{
				$"Some types of terrain are impassable.",
				$"At least on ground. This is where the fun begins.",
				$"",
				$"Press {Color.Cyan}Continue {Color.White}to proceed."
			}));
		}

		void tickAir()
		{
			if (world.LocalPlayer.TerrainPosition.Y <= 6 || world.LocalPlayer.TerrainPosition.X >= 19)
				return;

			Tick -= tickAir;
			Tick += tickSearch;
			Tick += tickJump;

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"There you go! Once your metamorphose is done again,",
				$"",
				$"{Color.Cyan}Move {Color.White}on the obvious spot!",
				$"I will bring you into a new environment."
			}));

			world.BeginPlayerSwitch(ActorCache.Types["human_playable"]);
			world.ShroudLayer.RevealShroudRectangular(Actor.PlayerTeam, new CPos(4 * 1024, 7 * 1024, 0), new CPos(17 * 1024, 16 * 1024, 0), true);
		}

		void tickJump()
		{
			if ((world.LocalPlayer.Position - new CPos(10 * 1024 + 512, 9 * 1024 + 512, 0)).SquaredFlatDist >= 1536 * 1536)
				return;

			world.LocalPlayer.Mobile.AccelerateHeight(60);
			world.LocalPlayer.Mobile.Accelerate(MathF.PI / 2f, 120);
		}

		void tickSearch()
		{
			if (world.LocalPlayer.Position.Y < 16 * 1024)
				return;

			Tick -= tickSearch;
			Tick -= tickJump;

			game.ScreenControl.ShowMessage(new Message(() => { }, new[]
			{
				$"Welcome to your new environment!",
				$"Now, {Color.Cyan}search {Color.White}for the key and then the exit!",
				$"This is one of the possible missions you can get.",
				$"After that, we are done with {Color.Red}Stage 1{Color.White}!"
			}));

			MusicController.FadeIntenseIn(60 * 60);
		}
	}
}