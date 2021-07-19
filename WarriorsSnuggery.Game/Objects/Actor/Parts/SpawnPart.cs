using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	public enum Occasion
	{
		DAMAGE,
		DEATH,
		TICK
	}

	public enum SpawnPartTypes
	{
		ACTOR,
		PARTICLE,
		WEAPON,
		NONE
	}

	[Desc("Spawns objects when the object takes damage.", "Without the health rule, this rule is useless.")]
	public class SpawnPartInfo : PartInfo
	{
		[Desc("Probability that the object will be spawned.")]
		public readonly float Probability = 1f;
		[Desc("Count of spawned objects.")]
		public readonly int Count;
		[Desc("Time distance between spawn of the objects in ticks.", "Used for the TICK occasion.")]
		public readonly int Tick;
		[Desc("Name of the object.")]
		public readonly string Name;

		[Desc("Object will inherit Team from the dead object.")]
		public readonly bool InheritsTeam;
		[Desc("Object will inherit Bot from the dead object.")]
		public readonly bool InheritsBot;

		[Desc("Type of the object.")]
		public readonly SpawnPartTypes Type;
		[Desc("Condition to spawn.")]
		public readonly Condition Condition;
		[Desc("Defines when the objects should be spawned.")]
		public readonly Occasion Occasion;

		[Desc("Offset from the center of idling object where the objects spawn.", "Z-coordinate will be used for height.")]
		public readonly CPos Offset;
		[Desc("Radius in which the objects get spawned randomly.", "If set to 0, physics radius will be used when possible.")]
		public readonly int Radius;
		[Desc("Threshold for damage concerning the DAMAGE occasion.")]
		public readonly int DamageThreshold = 2;

		[Desc("Sound to play when spawning.")]
		public readonly SoundType Sound;

		[Desc("Spawn object at center of actor, not random.")]
		public readonly bool AtCenter;

		public SpawnPartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new SpawnPart(self, this);
		}
	}

	public class SpawnPart : ActorPart, ITick, INoticeDamage, INoticeKilled, ISaveLoadable
	{
		readonly SpawnPartInfo info;
		int curTick;

		public SpawnPart(Actor self, SpawnPartInfo info) : base(self)
		{
			this.info = info;
		}

		public void OnLoad(PartLoader loader)
		{
			foreach (var node in loader.GetNodes(typeof(SpawnPart), info.InternalName))
			{
				if (node.Key == "Tick")
					curTick = node.Convert<int>();
			}
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			saver.Add("Tick", curTick, 0);

			return saver;
		}

		public void OnDamage(Actor damager, int damage)
		{
			if (info.Occasion == Occasion.DAMAGE && damage > info.DamageThreshold)
				create();
		}

		public void OnKilled(Actor killer)
		{
			if (info.Occasion == Occasion.DEATH)
				create();
		}

		public void Tick()
		{
			if (info.Type != SpawnPartTypes.PARTICLE)
				return;

			if (info.Occasion == Occasion.TICK && curTick-- < 0)
				create();
		}

		void create()
		{
			if (info.Condition == null || info.Condition.True(self))
			{
				curTick = info.Tick;
				if (info.Sound != null)
				{
					var sound = new Sound(info.Sound);
					sound.Play(self.Position, false);
				}
				for (int i = 0; i < info.Count; i++)
					createObject();

				return;
			}

			curTick = 0;
		}

		void createObject()
		{
			if (self.World.Game.SharedRandom.NextDouble() > info.Probability)
				return;

			var height = self.Height + info.Offset.Z;
			switch (info.Type)
			{
				case SpawnPartTypes.ACTOR:
					var actor = ActorCache.Create(self.World, info.Name, randomPosition(), info.InheritsTeam ? self.Team : Actor.NeutralTeam, info.InheritsBot && self.IsBot);

					if (info.InheritsBot && self.IsBot)
						actor.Bot.Target = self.Bot.Target;
					actor.Height = height;

					self.World.Add(actor);
					break;
				case SpawnPartTypes.PARTICLE:
					var particle = ParticleCache.Create(self.World, info.Name, randomPosition(), self.Height + info.Offset.Z);
					particle.Height = height;

					self.World.Add(particle);
					break;
				case SpawnPartTypes.WEAPON:
					var weapon = WeaponCache.Create(self.World, info.Name, randomPosition(), self);
					weapon.Height = height;

					self.World.Add(weapon);
					break;
				default:
					return;
			}
		}

		CPos randomPosition()
		{
			if (info.AtCenter)
				return self.Position + new CPos(info.Offset.X, info.Offset.Y, 0);

			var sizeX = info.Radius;
			var sizeY = info.Radius;

			if (sizeX == 0 && sizeY == 0 && !self.Physics.IsEmpty)
			{
				sizeX = self.Physics.Type.RadiusX;
				sizeY = self.Physics.Type.RadiusY;
			}

			var x = self.World.Game.SharedRandom.Next(-sizeX, sizeX);
			var y = self.World.Game.SharedRandom.Next(-sizeY, sizeY);
			return self.Position + new CPos(x, y, 0) + new CPos(info.Offset.X, info.Offset.Y, 0);
		}
	}
}
