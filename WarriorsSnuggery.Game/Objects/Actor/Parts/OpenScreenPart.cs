﻿using WarriorsSnuggery.Conditions;
using WarriorsSnuggery.Maps.Layers;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	public enum ScreenType
	{
		NEW_STORY_GAME,
		NEW_NORMAL_GAME,
		SPELL_SHOP,
		ACTOR_SHOP,
		LIFE_SHOP,
		TROPHY_COLLECTION,
		CREDITS
	}

	[Desc("Attach to an actor to open a screen when an actor gets near.")]
	public class OpenScreenPartInfo : PartInfo
	{
		[Desc("Selects to what type of screen to open.")]
		public readonly ScreenType Type = ScreenType.TROPHY_COLLECTION;
		[Desc("Scanradius for triggering.")]
		public readonly int Radius = 512;

		[Desc("Determines whether to pause the game.")]
		public readonly bool Pause = true;

		[Desc("Activate only by Player.")]
		public readonly bool OnlyByPlayer;
		[Desc("Activate only by the following Condition.")]
		public readonly Condition Condition;

		public OpenScreenPartInfo(PartInitSet set) : base(set) { }
	}

	public class OpenScreenPart : ActorPart, ITick, INoticeMove
	{
		readonly OpenScreenPartInfo info;
		bool activated;
		Actor lastActor;
		ActorSector[] sectors;
		bool firstTick = true;

		public OpenScreenPart(Actor self, OpenScreenPartInfo info) : base(self, info)
		{
			this.info = info;
		}

		public void Tick()
		{
			if (firstTick)
			{
				firstTick = false;
				updateSectors();
			}

			if (activated)
			{
				if ((lastActor.Position - Self.Position).SquaredFlatDist > info.Radius * info.Radius)
					activated = false;

				return;
			}

			if (info.Condition != null && !info.Condition.True(Self))
				return;

			if (info.OnlyByPlayer)
			{
				var localPlayer = Self.World.LocalPlayer;

				if (localPlayer != null && localPlayer.IsAlive && localPlayer.WorldPart != null && localPlayer.WorldPart.CanTrigger && (localPlayer.Position - Self.Position).SquaredFlatDist < info.Radius * info.Radius)
					activate(localPlayer);
			}
			else
			{
				var squared = info.Radius * info.Radius;
				foreach (var sector in sectors)
				{
					foreach (var actor in sector.Actors)
					{
						if (actor != Self && actor.IsAlive && actor.WorldPart != null && actor.WorldPart.CanTrigger && (actor.Position - Self.Position).SquaredFlatDist < squared)
							activate(actor);
					}
				}
			}

			void activate(Actor actor)
			{
				if (!invokeFunction(actor))
					return;

				activated = true;
				lastActor = actor;
			}

			bool invokeFunction(Actor a)
			{
				var game = a.World.Game;

				game.Pause(info.Pause);
				switch (info.Type)
				{
					case ScreenType.NEW_STORY_GAME:
						game.ShowScreen(UI.Screens.ScreenType.NEWSTORYGAME);

						return true;
					case ScreenType.NEW_NORMAL_GAME:
						game.ShowScreen(UI.Screens.ScreenType.NEWNORMALGAME);

						return true;
					case ScreenType.SPELL_SHOP:
						game.ShowScreen(UI.Screens.ScreenType.SPELLSHOP);

						return true;
					case ScreenType.ACTOR_SHOP:
						game.ShowScreen(UI.Screens.ScreenType.ACTORSHOP);

						return true;
					case ScreenType.LIFE_SHOP:
						game.ShowScreen(UI.Screens.ScreenType.LIFESHOP);

						return true;
					case ScreenType.TROPHY_COLLECTION:
						game.ShowScreen(UI.Screens.ScreenType.TROPHY);

						return true;
					case ScreenType.CREDITS:
						game.ShowScreen(UI.Screens.ScreenType.CREDITS);

						return true;
				}

				return false;
			}
		}

		public void OnMove(CPos old, CPos speed)
		{
			updateSectors();
		}

		void updateSectors()
		{
			sectors = Self.World.ActorLayer.GetSectors(Self.Position, info.Radius);
		}
	}
}
