using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Parts
{
	public enum CollectableType
	{
		NONE,
		MONEY,
		HEALTH,
		MANA,
		TEXT,
		KEY,
		TROPHY,
		LIFE
	}

	[Desc("Attach to an actor to make it trigger an effect when an actor gets near.")]
	public class CollectablePartInfo : PartInfo
	{
		[Desc("Type of the effect on triggering.")]
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

		[Desc("Value field for the effect.", "MONEY: amount of money.", "MANA: amount of mana.", "TEXT: display duration of the text.")]
		public readonly int Value;
		[Desc("Text lines for the effect.", "Commas are used to separate the lines.", "TEXT: text that gets displayed.", "TROPHY: name of the trophy.")]
		public readonly string[] Text;

		public CollectablePartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new CollectablePart(self, this);
		}
	}

	public class CollectablePart : ActorPart, ITick, INoticeMove, ISaveLoadable
	{
		readonly CollectablePartInfo info;
		bool activated;
		int cooldown;
		Actor lastActor;
		ActorSector[] sectors;
		bool firstTick = true;

		public CollectablePart(Actor self, CollectablePartInfo info) : base(self)
		{
			this.info = info;
		}

		public void OnLoad(List<TextNode> nodes)
		{
			var parent = nodes.FirstOrDefault(n => n.Key == "CollectablePart" && n.Value == info.InternalName);
			if (parent == null)
				return;

			foreach (var node in parent.Children)
			{
				if (node.Key == "Activated")
					activated = node.Convert<bool>();
				if (node.Key == "Cooldown")
					cooldown = node.Convert<int>();
			}
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			saver.Add("Activated", activated, false);
			saver.Add("Cooldown", cooldown, 0);

			return saver;
		}

		public void Tick()
		{
			if (activated && !info.MultipleActivations)
				return;

			if (firstTick)
			{
				firstTick = false;
				updateSectors();
			}

			if (activated)
			{
				if (info.Duration < 0)
				{
					if ((lastActor.Position - self.Position).SquaredFlatDist > info.Radius * info.Radius)
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

				if (localPlayer != null && localPlayer.IsAlive && localPlayer.WorldPart != null && localPlayer.WorldPart.CanTrigger && (localPlayer.Position - self.Position).SquaredFlatDist < info.Radius * info.Radius)
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
						if (info.Value > 0 && a.Health.HP == a.Health.MaxHP)
							return false;

						a.Health.HP += info.Value;

						return true;
					case CollectableType.MANA:
						a.World.Game.Stats.Mana += info.Value;

						return true;
					case CollectableType.MONEY:
						a.World.Game.Stats.Money += info.Value;

						return true;
					case CollectableType.TEXT:
						a.World.AddText(a.Position, info.Value, ActionText.ActionTextType.TRANSFORM, info.Text);

						return true;
					case CollectableType.KEY:
						a.World.Game.Stats.KeyFound = true;

						return true;
					case CollectableType.TROPHY:
						a.World.TrophyCollected(info.Text[0]);

						return true;
					case CollectableType.LIFE:
						a.World.Game.Stats.Lifes += info.Value;

						return true;
					default:
						return true;
				}
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
