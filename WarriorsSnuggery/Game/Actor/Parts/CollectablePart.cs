using System;
using WarriorsSnuggery.Graphics;

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
		public readonly bool KillSelf;
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

		public override ActorPart Create(Actor self)
		{
			return new CollectablePart(self, this);
		}

		public CollectablePartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Type":
						Type = (CollectableType) node.ToEnum(typeof(CollectableType));

						break;
					case "Radius":
						Radius = node.ToInt();

						break;
					case "OnlyByPlayer":
						OnlyByPlayer = node.ToBoolean();

						break;
					case "KillsSelf":
						KillSelf = node.ToBoolean();

						break;
					case "MultipleActivations":
						MultipleActivations = node.ToBoolean();

						break;
					case "Duration":
						Duration = node.ToInt();

						break;
					case "Value":
						Value = node.ToInt();

						break;
					case "Text":
						Text = node.Value;

						break;
					case "ParticleSpawner":
						ParticleSpawner = node.ToParticleSpawner();

						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "CollectablePart");
				}
			}
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

				if (localPlayer != null && self.World.PlayerAlive && localPlayer.WorldPart != null && localPlayer.WorldPart.CanTrigger && localPlayer.Position.GetDistToXY(self.Position) < info.Radius)
				{
					activate(localPlayer);
				}
			}
			else
			{
				foreach (var actor in self.World.Actors)
				{
					if (actor != self && actor.IsAlive && actor.WorldPart != null && actor.WorldPart.CanTrigger && actor.Position.GetDistToXY(self.Position) < info.Radius)
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
					foreach(var particle in info.ParticleSpawner.Create(self.Position))
						self.World.Add(particle);
				}

				if (info.KillSelf)
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
							a.World.Game.Statistics.Mana += info.Value;
							if (a.World.Game.Statistics.Mana > a.World.Game.Statistics.MaxMana)
								a.World.Game.Statistics.Mana = a.World.Game.Statistics.MaxMana;
						};

					case CollectableType.MONEY:
						return (a) => a.World.Game.Statistics.Money += info.Value;

					case CollectableType.NEXT_LEVEL:
						return (a) =>
						{
							if (a.World.Game.Type == GameType.TEST)
							{
								a.World.Game.End = true;
								a.World.Game.NewGameType = GameType.MAINMENU;
							}
							else
							{
								a.World.Game.Statistics.Money += info.Value;
								a.World.Game.ScreenControl.ShowScreen(UI.ScreenType.WIN);
								a.World.Game.Pause(true);
							}
						};
					case CollectableType.NEXT_LEVEL_INSTANT:
						return (a) =>
						{
							a.World.Game.End = true;
							if (a.World.Game.Type == GameType.TEST)
							{
								a.World.Game.NewGameType = GameType.MAINMENU;
							}
							else
							{
								a.World.Game.Statistics.Money += info.Value;
								a.World.Game.NewGameType = GameType.NORMAL;
							}
						};

					case CollectableType.TUTORIAL_LEVEL:
						return (a) =>
						{
							a.World.Game.End = true;
							a.World.Game.Statistics.Money += info.Value;
							a.World.Game.NewGameType = GameType.TUTORIAL;
						};

					case CollectableType.MAIN_LEVEL:
						return (a) =>
						{
							a.World.Game.End = true;
							a.World.Game.Statistics.Money += info.Value;
							a.World.Game.NewGameType = GameType.MENU;
						};

					case CollectableType.MAINMENU_LEVEL:
						return (a) =>
						{
							a.World.Game.End = true;
							a.World.Game.Statistics.Money += info.Value;
							a.World.Game.NewGameType = GameType.MAINMENU;
						};

					case CollectableType.NEW_STORY_GAME:
						return (a) =>
						{
							a.World.Game.Pause(true);
							a.World.Game.ScreenControl.ShowScreen(UI.ScreenType.NEW_STORY_GAME);
						};

					case CollectableType.NEW_CUSTOM_GAME:
						return (a) =>
						{
							a.World.Game.Pause(true);
							a.World.Game.ScreenControl.ShowScreen(UI.ScreenType.NEW_CUSTOM_GAME);
						};

					case CollectableType.TEXT:
						return (a) => a.World.Add(new ActionText(a.Position + new CPos(0, 0, 1024), IFont.Pixel16, new CPos(0, -15, 30), 100, info.Text));

					case CollectableType.TECH_TREE:
						return (a) =>
						{
							a.World.Game.Pause(true);
							a.World.Game.ScreenControl.ShowScreen(UI.ScreenType.TECHTREE);
						};

					default:
						return (a) => { };
				}
			}
		}
	}
}
