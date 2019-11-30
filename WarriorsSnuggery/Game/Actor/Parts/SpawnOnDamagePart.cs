using WarriorsSnuggery.Objects.Conditions;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Spawns objects when the object takes damage.", "Without the health rule, this rule is useless.")]
	public class SpawnOnDamagePartInfo : PartInfo
	{
		[Desc("Probability that the object will be spawned.")]
		public readonly float Probability = 1f;
		[Desc("Count of spawned objects.")]
		public readonly int Count;
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

		[Desc("Spawn object at center of actor, not random.")]
		public readonly bool AtCenter;

		public override ActorPart Create(Actor self)
		{
			return new SpawnOnDamagePart(self, this);
		}

		public SpawnOnDamagePartInfo(MiniTextNode[] nodes) : base(nodes)
		{

		}
	}

	public class SpawnOnDamagePart : ActorPart
	{
		readonly SpawnOnDamagePartInfo info;

		public SpawnOnDamagePart(Actor self, SpawnOnDamagePartInfo info) : base(self)
		{
			this.info = info;
		}

		void create()
		{
			if ((info.Condition != null && !info.Condition.True(self)) || self.World.Game.SharedRandom.NextDouble() > info.Probability)
				return;

			PhysicsObject @object;
			switch (info.Type)
			{
				case "ACTOR":
					@object = ActorCreator.Create(self.World, info.Name, randomPosition(), info.InheritsTeam ? self.Team : Actor.NeutralTeam, info.InheritsBot ? self.IsBot : false);
					break;
				case "PARTICLE":
					@object = ParticleCreator.Create(info.Name, randomPosition(), self.Height, self.World.Game.SharedRandom);
					break;
				case "WEAPON":
					@object = WeaponCreator.Create(self.World, info.Name, randomPosition(), randomPosition());
					break;
				default:
					throw new YamlInvalidNodeException(string.Format("SpawnOnDeath does not create objects of class '{0}'.", info.Type));
			}
			self.World.Add(@object);
		}

		CPos randomPosition()
		{
			if (info.AtCenter)
				return self.Position;

			var size = self.Physics != null ? self.Physics.RadiusX : 40;
			var x = Program.SharedRandom.Next(size) - size / 2;
			var y = Program.SharedRandom.Next(size) - size / 2;
			return self.Position + new CPos(x, y, 0);
		}

		public override void OnDamage(Actor damager, int damage)
		{
			if (damage > 0)
			{
				for (int i = 0; i < info.Count; i++)
				{
					create();
				}
			}
		}
	}
}
