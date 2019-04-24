using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarriorsSnuggery.Objects.Parts
{
	public class SpawnOnDamagePartInfo : PartInfo
	{
		public readonly float Probability = 1f;
		public readonly int Count;
		public readonly string Name;
		public readonly bool InheritsTeam;
		public readonly bool InheritsBot;
		public readonly string Type;
		public readonly string Condition;

		public override ActorPart Create(Actor self)
		{
			return new SpawnOnDamagePart(self, this);
		}

		public SpawnOnDamagePartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Probability":
						Probability = node.ToFloat();
						break;
					case "Count":
						Count = node.ToInt();
						break;
					case "Name":
						Name = node.Value;
						break;
					case "Type":
						Type = node.Value;
						break;
					case "Condition":
						Condition = node.Value;
						break;
					case "InheritsTeam":
						InheritsTeam = node.ToBoolean();
						break;
					case "InheritsBot":
						InheritsBot = node.ToBoolean();
						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "SpawnOnDeathPart");
				}
			}
		}
	}

	/// <summary>
	/// Spawns objects when the player takes damage.
	/// </summary>
	public class SpawnOnDamagePart : ActorPart
	{
		readonly SpawnOnDamagePartInfo info;

		public SpawnOnDamagePart(Actor self, SpawnOnDamagePartInfo info) : base(self)
		{
			this.info = info;
		}

		void create()
		{
			if (self.World.Game.SharedRandom.NextDouble() > info.Probability)
				return;

			PhysicsObject @object = null;
			switch (info.Type)
			{
				case "ACTOR":
					@object = ActorCreator.Create(self.World, info.Name, randomPosition(), info.InheritsTeam ? self.Team : 0, info.InheritsBot ? self.IsBot : false);
					break;
				case "PARTICLE":
					@object = ParticleCreator.Create(info.Name, randomPosition());
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
			var size = self.Physics != null ? self.Physics.Radius : 40;
			var x = Program.SharedRandom.Next(size) - size / 2;
			var y = Program.SharedRandom.Next(size) - size / 2;
			return self.Position + new CPos(x, y, 0);
		}

		public override void OnDamage(Actor damager, int damage)
		{
			if (damage > 0)
			{
				create();
			}
		}
	}
}
