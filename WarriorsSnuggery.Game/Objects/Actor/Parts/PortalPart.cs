using WarriorsSnuggery.Conditions;
using WarriorsSnuggery.Maps.Layers;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	public enum PortalType
	{
		NEXT_LEVEL,
		TUTORIAL_LEVEL,
		MAIN_LEVEL,
		MAINMENU_LEVEL
	}

	[Desc("Attach to an actor to make it trigger a level transfer when an actor gets near.")]
	public class PortalPartInfo : PartInfo
	{
		[Desc("Selects to what type of level to go next.")]
		public readonly PortalType Type = PortalType.NEXT_LEVEL;
		[Desc("Scanradius for triggering.")]
		public readonly int Radius = 512;

		[Desc("Determines whether to skip the win screen and continue immediately.")]
		public readonly bool Instant;

		[Desc("Determines whether to increase the level number.")]
		public readonly bool IncreaseLevel;

		[Desc("Activate only by Player.")]
		public readonly bool OnlyByPlayer;
		[Desc("Activate only by the following Condition.")]
		public readonly Condition Condition;

		public PortalPartInfo(PartInitSet set) : base(set) { }
	}

	public class PortalPart : ActorPart, ITick, INoticeMove
	{
		readonly PortalPartInfo info;
		bool activated;
		Actor lastActor;
		ActorSector[] sectors;
		bool firstTick = true;

		public PortalPart(Actor self, PortalPartInfo info) : base(self, info)
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
				switch (info.Type)
				{
					case PortalType.NEXT_LEVEL:
						if (game.MissionType == MissionType.TEST)
						{
							game.ChangeLevelAfterTick(MissionType.MAIN_MENU, InteractionMode.NONE, info.IncreaseLevel);
							return true;
						}

						if (info.Instant)
						{
							game.ChangeLevelAfterTick(game.MissionType.GetCampaignType(), increaseLevel: info.IncreaseLevel);
							return true;
						}

						game.VictoryConditionsMet(true);
						return true;
					case PortalType.TUTORIAL_LEVEL:
						game.ChangeLevelAfterTick(MissionType.TUTORIAL, increaseLevel: info.IncreaseLevel);

						return true;
					case PortalType.MAIN_LEVEL:

						game.ChangeLevelAfterTick(game.MissionType.GetMenuType(), increaseLevel: info.IncreaseLevel);

						return true;
					case PortalType.MAINMENU_LEVEL:
						game.ChangeLevelAfterTick(MissionType.MAIN_MENU, InteractionMode.NONE, info.IncreaseLevel);

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
