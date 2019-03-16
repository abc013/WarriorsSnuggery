using System;

namespace WarriorsSnuggery.Objects.Parts
{
	public enum CollectableType
	{
		NONE = 0,
		MONEY = 1,
		HEALTH = 2,
		MANA = 3,
		NEXT_LEVEL = 4,
		TUTORIAL_LEVEL = 5,
		MAIN_LEVEL = 6,
		MAINMENU_LEVEL = 7,
		TEXT = 8,
		SPAWNOBJECT = 9,
		NEW_GAME = 10,
		NEW_STORY_GAME = 11,
		NEW_CUSTOM_GAME = 12
	}

	public class CollectablePartInfo : PartInfo
	{
		public readonly CollectableType Type;
		public readonly int Radius;

		public readonly bool MultipleActivations;
		public readonly bool KillSelf;
		public readonly int ActivationDuration;

		public readonly bool ActivateByPlayer;

		public readonly ParticleSpawner Particles;

		public readonly int Value;
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
						ActivateByPlayer = node.ToBoolean();

						break;
					case "KillsSelf":
						KillSelf = node.ToBoolean();

						break;
					case "MultipleActivations":
						MultipleActivations = node.ToBoolean();

						if (node.Children.Exists(c => c.Key == "Duration"))
						{
							ActivationDuration = node.Children.Find(c => c.Key == "Duration").ToInt();
						}
						break;
					case "Value":
						Value = node.ToInt();

						break;
					case "Text":
						Text = node.Value;

						break;
					case "ParticleSpawner":
						Particles = node.ToParticleSpawner();

						break;
					default:
						throw new YamlUnknownNodeException(node.Key);
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

			if (info.ActivateByPlayer)
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
				cooldown = info.ActivationDuration;

				if (info.Particles != null)
				{
					foreach(var particle in info.Particles.Create(self.Position))
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
						return (a) => a.World.Game.Statistics.Mana += info.Value;

					case CollectableType.MONEY:
						return (a) => a.World.Game.Statistics.Money += info.Value;

					case CollectableType.NEXT_LEVEL:
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
						return (a) => a.World.Add(new ActionText(a.Position + new CPos(0, 0, 1024), info.Text, Color.White, IFont.Pixel16, new CPos(0, -15, 30)));

					default:
						return (a) => { };
				}
			}
		}
	}
}
