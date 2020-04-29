using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Physics;

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
		TECH_TREE,
		KEY
	}

	[Desc("Attach to an actor to make it trigger an effect when an actor gets near.")]
	public class CollectablePartInfo : PartInfo
	{
		[Desc("Type of the effect on triggering.", "Possible: NONE, MONEY, HEALTH, NEXT_LEVEL, NEXT_LEVEL_INSTANT, TUTORIAL_LEVEL, MAIN_LEVEL, MAINMENU_LEVEL, TEXT, SPAWNOBJECT, NEW_GAME, NEW_STORY_GAME, NEW_CUSTOM_GAME, TECH_TREE, KEY;")]
		public readonly CollectableType Type = CollectableType.NONE;
		[Desc("Scanradius for triggering.")]
		public readonly int Radius = 512;

		[Desc("Allow multiple activations.")]
		public readonly bool MultipleActivations;
		[Desc("Trigger kills itself upon collection.")]
		public readonly bool KillsSelf;
		[Desc("Time until the trigger can be reactivated again.", "If set to negative value, the actor has to leave and enter the radius to activate again.")]
		public readonly int Duration = -1;

		[Desc("Activate only by Player.")]
		public readonly bool OnlyByPlayer;
		[Desc("Activate only by the following Condition.")]
		public readonly Condition Condition;

		[Desc("Spawn particles when triggered.")]
		public readonly ParticleSpawner ParticleSpawner;
		[Desc("Play sound when triggered.")]
		public readonly SoundType Sound;

		[Desc("Value field for the effect.")]
		public readonly int Value;
		[Desc("Text lines for the effect.", "Commas are used to separate the lines.")]
		public readonly string[] Text;

		public CollectablePartInfo(string internalName, MiniTextNode[] nodes) : base(internalName, nodes) { }

		public override ActorPart Create(Actor self)
		{
			return new CollectablePart(self, this);
		}
	}

	public class CollectablePart : ActorPart
	{
		readonly CollectablePartInfo info;
		readonly SimplePhysics physics;
		bool activated;
		int cooldown;
		Actor lastActor;
		bool updatePhysics = true;
		PhysicsSector[] sectors;

		public CollectablePart(Actor self, CollectablePartInfo info) : base(self)
		{
			this.info = info;
			physics = new SimplePhysics(self.Position, 0, Shape.CIRCLE, info.Radius, info.Radius, info.Radius);
		}

		public override void Tick()
		{
			if (self.World.Game.Type == GameType.EDITOR)
				return;

			if (activated && !info.MultipleActivations)
				return;

			if (activated)
			{
				if (info.Duration < 0)
				{
					if ((lastActor.Position - self.Position).FlatDist > info.Radius)
						activated = false;
				}
				else
					activated &= --cooldown > 0;

				return;
			}

			if (info.Condition != null && !info.Condition.True(self))
				return;

			if (info.OnlyByPlayer)
			{
				var localPlayer = self.World.LocalPlayer;

				if (localPlayer != null && self.World.PlayerAlive && localPlayer.WorldPart != null && localPlayer.WorldPart.CanTrigger && (localPlayer.Position - self.Position).FlatDist < info.Radius)
					activate(localPlayer);
			}
			else
			{
				if (updatePhysics)
				{
					physics.Position = self.Position;
					physics.Height = self.Height;
					sectors = self.World.PhysicsLayer.GetSectors(physics);
					updatePhysics = false;
				}

				foreach (var sector in sectors)
				{
					foreach (var @object in sector.GetObjects())
					{
						if (!(@object is Actor actor))
							continue;

						if (actor != self && actor.IsAlive && actor.WorldPart != null && actor.WorldPart.CanTrigger && (actor.Position - self.Position).FlatDist < info.Radius)
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
				cooldown = info.Duration;

				if (info.ParticleSpawner != null)
					self.World.Add(info.ParticleSpawner.Create(self.World, self.Position, self.Height));

				if (info.Sound != null)
				{
					var sound = new Sound(info.Sound);
					sound.Play(self.Position, false);
				}

				if (info.KillsSelf)
					self.Killed(null);
			}

			bool invokeFunction(Actor a)
			{
				var game = a.World.Game;
				switch (info.Type)
				{
					case CollectableType.HEALTH:
						if (a.Health == null)
							return false;
						if (a.Health.HP == a.Health.MaxHP)
							return false;
						
						a.Health.HP += info.Value;

						return true;
					case CollectableType.MANA:
						var stats = a.World.Game.Statistics;
						if (stats.Mana == stats.MaxMana)
							return false;

						stats.Mana += info.Value;
						if (stats.Mana > stats.MaxMana)
							stats.Mana = stats.MaxMana;

						return true;
					case CollectableType.MONEY:
						a.World.Game.Statistics.Money += info.Value;

						return true;
					case CollectableType.NEXT_LEVEL:
						if (game.Type == GameType.TEST)
						{
							game.InstantLevelChange(GameType.MAINMENU);
						}
						else
						{
							game.Statistics.Money += info.Value;
							game.VictoryConditionsMet();
						}
						return true;
					case CollectableType.NEXT_LEVEL_INSTANT:
						if (game.Type == GameType.TEST)
						{
							game.InstantLevelChange(GameType.MAINMENU);
						}
						else
						{
							game.Statistics.Money += info.Value;
							game.InstantLevelChange(GameType.NORMAL);
						}

						return true;
					case CollectableType.TUTORIAL_LEVEL:
						game.InstantLevelChange(GameType.TUTORIAL);

						return true;
					case CollectableType.MAIN_LEVEL:
						game.Statistics.Money += info.Value;
						game.InstantLevelChange(GameType.MENU);

						return true;
					case CollectableType.MAINMENU_LEVEL:
						game.InstantLevelChange(GameType.MAINMENU);

						return true;
					case CollectableType.NEW_STORY_GAME:
						game.Pause(true);
						game.ScreenControl.ShowScreen(UI.ScreenType.NEW_STORY_GAME);

						return true;
					case CollectableType.NEW_CUSTOM_GAME:
						game.Pause(true);
						game.ScreenControl.ShowScreen(UI.ScreenType.NEW_CUSTOM_GAME);

						return true;
					case CollectableType.TECH_TREE:
						game.Pause(true);
						game.ScreenControl.ShowScreen(UI.ScreenType.TECHTREE);

						return true;
					case CollectableType.TEXT:
						a.World.Add(new ActionText(a.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), info.Value, ActionText.ActionTextType.TRANSFORM, info.Text));

						return true;
					case CollectableType.KEY:
						a.World.KeyFound = true;

						return true;
					default:
						return true;
				}
			}
		}

		public override void OnMove(CPos old, CPos speed)
		{
			updatePhysics = true;
		}
	}
}
