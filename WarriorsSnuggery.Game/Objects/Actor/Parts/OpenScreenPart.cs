using System.Collections.Generic;
using WarriorsSnuggery.Objects.Conditions;

namespace WarriorsSnuggery.Objects.Parts
{
	public enum ScreenType
	{
		NEW_STORY_GAME,
		NEW_NORMAL_GAME,
		SPELL_SHOP,
		ACTOR_SHOP,
		TROPHY_COLLECTION
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

		public OpenScreenPartInfo(string internalName, List<MiniTextNode> nodes) : base(internalName, nodes) { }

		public override ActorPart Create(Actor self)
		{
			return new OpenScreenPart(self, this);
		}
	}

	public class OpenScreenPart : ActorPart, ITick, INoticeMove
	{
		readonly OpenScreenPartInfo info;
		bool activated;
		Actor lastActor;
		ActorSector[] sectors;
		bool firstTick = true;

		public OpenScreenPart(Actor self, OpenScreenPartInfo info) : base(self)
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
				if ((lastActor.Position - self.Position).SquaredFlatDist > info.Radius * info.Radius)
					activated = false;

				return;
			}

			if (info.Condition != null && !info.Condition.True(self))
				return;

			if (info.OnlyByPlayer)
			{
				var localPlayer = self.World.LocalPlayer;

				if (localPlayer != null && self.World.PlayerAlive && localPlayer.WorldPart != null && localPlayer.WorldPart.CanTrigger && (localPlayer.Position - self.Position).SquaredFlatDist < info.Radius * info.Radius)
					activate(localPlayer);
			}
			else
			{
				var squared = info.Radius * info.Radius;
				foreach (var sector in sectors)
				{
					foreach (var actor in sector.Actors)
					{
						if (actor != self && actor.IsAlive && actor.WorldPart != null && actor.WorldPart.CanTrigger && (actor.Position - self.Position).SquaredFlatDist < squared)
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
					case ScreenType.TROPHY_COLLECTION:
						game.ShowScreen(UI.Screens.ScreenType.TROPHY);

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
			sectors = self.World.ActorLayer.GetSectors(self.Position, info.Radius);
		}
	}
}
