using WarriorsSnuggery.Objects.Conditions;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Spawns objects when the object idles.")]
	public class SpawnOnIdlePartInfo : PartInfo
	{
		[Desc("Probability that the object will be spawned.")]
		public readonly float Probability = 1f;
		[Desc("Count of spawned objects.")]
		public readonly int Count;
		[Desc("Time distance between spawn of the objects in ticks.")]
		public readonly int Tick;
		[Desc("Name of the object.")]
		public readonly string Name;
		[Desc("Object will inherit Team from the dead object.")]
		public readonly bool InheritsTeam;
		[Desc("Object will inherit Bot from the dead object.")]
		public readonly bool InheritsBot;
		[Desc("Type of the object.")]
		public readonly string Type;
		[Desc("Condition to spawn.")]
		public readonly Condition Condition;
		[Desc("Offset from the center of idling object where the objects spawn.", "Z-coordinate will be used for height.")]
		public readonly CPos Offset;

		[Desc("Spawn object at center of actor, not random.")]
		public readonly bool AtCenter;

		public override ActorPart Create(Actor self)
		{
			return new SpawnOnIdlePart(self, this);
		}

		public SpawnOnIdlePartInfo(MiniTextNode[] nodes) : base(nodes)
		{

		}
	}

	public class SpawnOnIdlePart : ActorPart
	{
		readonly SpawnOnIdlePartInfo info;
		int curTick;

		public SpawnOnIdlePart(Actor self, SpawnOnIdlePartInfo info) : base(self)
		{
			this.info = info;
		}

		public override void Tick()
		{
			if ((info.Condition != null && !info.Condition.True(self)) || self.CurrentAction == ActorAction.IDLING && curTick-- < 0)
			{
				for (int i = 0; i < info.Count; i++)
				{
					create();
				}
			}
		}

		void create()
		{
			curTick = info.Tick;

			if (self.World.Game.SharedRandom.NextDouble() > info.Probability)
				return;

			PhysicsObject @object;
			switch (info.Type)
			{
				case "ACTOR":
					@object = ActorCreator.Create(self.World, info.Name, randomPosition(), info.InheritsTeam ? self.Team : Actor.NeutralTeam, info.InheritsBot ? self.IsBot : false);

					if (self.IsBot)
						((Actor)@object).BotPart.Target = self.BotPart.Target;
					break;
				case "PARTICLE":
					@object = ParticleCreator.Create(info.Name, randomPosition(), self.Height + info.Offset.Z, self.World.Game.SharedRandom);
					break;
				case "WEAPON":
					@object = WeaponCreator.Create(self.World, info.Name, randomPosition(), self);
					break;
				default:
					throw new YamlInvalidNodeException(string.Format("SpawnOnIdle does not create objects of class '{0}'.", info.Type));
			}
			@object.Height = self.Height + info.Offset.Z;
			self.World.Add(@object);
		}

		CPos randomPosition()
		{
			if (info.AtCenter)
				return self.Position + new CPos(info.Offset.X, info.Offset.Y, 0);

			var size = self.Physics != null ? self.Physics.RadiusX : 40;
			var x = Program.SharedRandom.Next(size) - size / 2;
			var y = Program.SharedRandom.Next(size) - size / 2;
			return self.Position + new CPos(x, y, 0) + new CPos(info.Offset.X, info.Offset.Y, 0);
		}
	}
}
