using System;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Parts
{
	public enum CollectableType
	{
		NONE,
		MONEY,
		HEALTH,
		MANA,
		NEXT_LEVEL,
		NEXT_LEVEL_INSTANT,
		TUTORIAL_LEVEL,
		MAIN_LEVEL,
		MAINMENU_LEVEL,
		TEXT,
		SPAWNOBJECT,
		NEW_GAME,
		NEW_STORY_GAME,
		NEW_CUSTOM_GAME,
		TECH_TREE
	}

	[Desc("Attach to an actor to make it trigger an effect when an actor gets near.")]
	public class CollectablePartInfo : PartInfo
	{
		[Desc("Type of the effect on triggering.")]
		public readonly CollectableType Type;
		[Desc("Scanradius for triggering.")]
		public readonly int Radius;

		[Desc("Allow multiple activations.")]
		public readonly bool MultipleActivations;
		[Desc("Trigger kills itself upon collection.")]
		public readonly bool KillsSelf;
		[Desc("Time until the trigger can be reactivated again.")]
		public readonly int Duration;

		[Desc("Activate only by Player.")]
		public readonly bool OnlyByPlayer;

		[Desc("Spawn particles when triggered.")]
		public readonly ParticleSpawner ParticleSpawner;

		[Desc("Value field for the effect.")]
		public readonly int Value;
		[Desc("Text field for the effect.")]
		public readonly string Text;

		public CollectablePartInfo(MiniTextNode[] nodes) : base(nodes) { }

		public override ActorPart Create(Actor self)
		{
			return new CollectablePart(self, this);
		}
	}

	public class CollectablePart : ActorPart
	{
		readonly CollectablePartInfo info;
		bool activated;
		int cooldown;

		public CollectablePart(Actor self, CollectablePartInfo info) : base(self)
		{
			this.info = info;
		}

		public override void Tick()
		{
			if (self.World.Game.Type == GameType.EDITOR)
				return;

			if (activated && !info.MultipleActivations)
				return;

			if (activated)
			{
				cooldown--;
				activated &= cooldown > 0;
				return;
			}

			if (info.OnlyByPlayer)
			{
				var localPlayer = self.World.LocalPlayer;

				if (localPlayer != null && self.World.PlayerAlive && localPlayer.WorldPart != null && localPlayer.WorldPart.CanTrigger && localPlayer.Position.DistToXY(self.Position) < info.Radius)
				{
					activate(localPlayer);
				}
			}
			else
			{
				foreach (var actor in self.World.Actors)
				{
					if (actor != self && actor.IsAlive && actor.WorldPart != null && actor.WorldPart.CanTrigger && actor.Position.DistToXY(self.Position) < info.Radius)
					{
						activate(actor);
					}
				}
			}

			void activate(Actor actor)
			{
				getFunction().Invoke(actor);

				activated = true;
				cooldown = info.Duration;

				if (info.ParticleSpawner != null)
				{
					foreach (var particle in info.ParticleSpawner.Create(self.World, self.Position, actor.Height))
						self.World.Add(particle);
				}

				if (info.KillsSelf)
					self.Killed(null);
			}

			Action<Actor> getFunction()
			{
				switch (info.Type)
				{
					case CollectableType.HEALTH:
						return (a) =>
						{
							if (a.Health != null)
								a.Health.HP += info.Value;
						};

					case CollectableType.MANA:
						return (a) =>
						{
							var stats = a.World.Game.Statistics;
							stats.Mana += info.Value;
							if (stats.Mana > stats.MaxMana)
								stats.Mana = stats.MaxMana;
						};

					case CollectableType.MONEY:
						return (a) => a.World.Game.Statistics.Money += info.Value;

					case CollectableType.NEXT_LEVEL:
						return (a) =>
						{
							var game = a.World.Game;
							if (game.Type == GameType.TEST)
							{
								game.InstantLevelChange(GameType.MAINMENU);
							}
							else
							{
								game.Statistics.Money += info.Value;
								game.VictoryConditionsMet();
							}
						};
					case CollectableType.NEXT_LEVEL_INSTANT:
						return (a) =>
						{
							var game = a.World.Game;
							if (game.Type == GameType.TEST)
							{
								game.InstantLevelChange(GameType.MAINMENU);
							}
							else
							{
								game.Statistics.Money += info.Value;
								game.InstantLevelChange(GameType.NORMAL);
							}
						};

					case CollectableType.TUTORIAL_LEVEL:
						return (a) =>
						{
							var game = a.World.Game;
							game.InstantLevelChange(GameType.TUTORIAL);
						};

					case CollectableType.MAIN_LEVEL:
						return (a) =>
						{
							var game = a.World.Game;
							game.Statistics.Money += info.Value;
							game.InstantLevelChange(GameType.MENU);
						};

					case CollectableType.MAINMENU_LEVEL:
						return (a) =>
						{
							var game = a.World.Game;
							game.InstantLevelChange(GameType.MAINMENU);
						};

					case CollectableType.NEW_STORY_GAME:
						return (a) =>
						{
							var game = a.World.Game;
							game.Pause(true);
							game.ScreenControl.ShowScreen(UI.ScreenType.NEW_STORY_GAME);
						};

					case CollectableType.NEW_CUSTOM_GAME:
						return (a) =>
						{
							var game = a.World.Game;
							game.Pause(true);
							game.ScreenControl.ShowScreen(UI.ScreenType.NEW_CUSTOM_GAME);
						};

					case CollectableType.TECH_TREE:
						return (a) =>
						{
							var game = a.World.Game;
							game.Pause(true);
							game.ScreenControl.ShowScreen(UI.ScreenType.TECHTREE);
						};

					case CollectableType.TEXT:
						return (a) => a.World.Add(new ActionText(a.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), 100, ActionText.ActionTextType.TRANSFORM, info.Text));

					default:
						return (a) => { };
				}
			}
		}
	}
}
