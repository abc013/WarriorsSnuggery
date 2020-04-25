using WarriorsSnuggery.Objects.Conditions;

namespace WarriorsSnuggery.Objects.Parts
{
	public enum Occasion
	{
		DAMAGE,
		DEATH,
		TICK
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

		[Desc("Type of the object.", "This can be set either to ACTOR, PARTICLE or WEAPON.")]
		public readonly string Type;
		[Desc("Condition to spawn.")]
		public readonly Condition Condition;
		[Desc("Defines when the objects should be spawned.", "possible: DAMAGE, DEATH, TICK")]
		public readonly Occasion Occasion;

		[Desc("Offset from the center of idling object where the objects spawn.", "Z-coordinate will be used for height.")]
		public readonly CPos Offset;
		[Desc("Radius in which the objects get spawned randomly.", "If set to 0, physics radius will be used when possible.")]
		public readonly int Radius;
		[Desc("Threshold for damage concerning the DAMAGE occasion.")]
		public readonly int DamageThreshold = 2;

		[Desc("Spawn object at center of actor, not random.")]
		public readonly bool AtCenter;

		public override ActorPart Create(Actor self)
		{
			return new SpawnPart(self, this);
		}

		public SpawnPartInfo(string internalName, MiniTextNode[] nodes) : base(internalName, nodes)
		{

		}
	}

	public class SpawnPart : ActorPart
	{
		readonly SpawnPartInfo info;
		int curTick;

		public SpawnPart(Actor self, SpawnPartInfo info) : base(self)
		{
			this.info = info;
		}

		public override void OnDamage(Actor damager, int damage)
		{
			if (info.Occasion == Occasion.DAMAGE && damage > info.DamageThreshold)
				create();
		}

		public override void OnKilled(Actor killer)
		{
			if (info.Occasion == Occasion.DEATH)
				create();
		}

		public override void Tick()
		{
			if (info.Occasion == Occasion.TICK && curTick-- < 0)
				create();
		}

		void create()
		{
			curTick = info.Tick;
			if (info.Condition == null || info.Condition.True(self))
			{
				for (int i = 0; i < info.Count; i++)
					createParticle();
			}
		}

		void createParticle()
		{
			if (self.World.Game.SharedRandom.NextDouble() > info.Probability)
				return;

			PhysicsObject @object;
			switch (info.Type)
			{
				case "ACTOR":
					var actor = ActorCreator.Create(self.World, info.Name, randomPosition(), info.InheritsTeam ? self.Team : Actor.NeutralTeam, info.InheritsBot && self.IsBot);

					if (info.InheritsBot && self.IsBot)
						actor.BotPart.Target = self.BotPart.Target;
					@object = actor;
					break;
				case "PARTICLE":
					@object = ParticleCreator.Create(info.Name, randomPosition(), self.Height + info.Offset.Z, self.World.Game.SharedRandom);
					break;
				case "WEAPON":
					@object = WeaponCreator.Create(self.World, info.Name, randomPosition(), self);
					break;
				default:
					throw new YamlInvalidNodeException(string.Format("Spawn does not create objects of type '{0}'.", info.Type));
			}
			@object.Height = self.Height + info.Offset.Z;
			self.World.Add(@object);
		}

		CPos randomPosition()
		{
			if (info.AtCenter)
				return self.Position + new CPos(info.Offset.X, info.Offset.Y, 0);

			var size = info.Radius == 0 ? (self.Physics != null ? self.Physics.RadiusX : 512) : info.Radius;
			var x = Program.SharedRandom.Next(size) - size / 2;
			var y = Program.SharedRandom.Next(size) - size / 2;
			return self.Position + new CPos(x, y, 0) + new CPos(info.Offset.X, info.Offset.Y, 0);
		}
	}
}
