using System;

namespace WarriorsSnuggery.Objects.Parts
{
	public class SpawnOnDeathPartInfo : PartInfo
	{
		public readonly int Count;
		public readonly string Name;
		public readonly bool InheritsTeam;
		public readonly bool InheritsBot;
		public readonly string Type;
		public readonly string Condition;

		public override ActorPart Create(Actor self)
		{
			return new SpawnOnDeathPart(self, this);
		}

		public SpawnOnDeathPartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach(var node in nodes)
			{
				switch(node.Key)
				{
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
						throw new YamlUnknownNodeException(node.Key);
				}
			}
		}
	}

	/// <summary>
	/// Spawns objects after death of the player.
	/// </summary>
	public class SpawnOnDeathPart : ActorPart
	{
		readonly SpawnOnDeathPartInfo info;

		public SpawnOnDeathPart(Actor self, SpawnOnDeathPartInfo info) : base(self)
		{
			this.info = info;
		}

		public override void OnKilled(Actor killer)
		{
			for(int i = 0; i < info.Count; i++)
			{
				create();
			}
		}

		void create()
		{
			GameObject @object = null;
			switch(info.Type)
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
			var size = self.Physics.Radius;
			var x = Program.SharedRandom.Next(size) - size / 2;
			var y = Program.SharedRandom.Next(size) - size / 2;
			return self.Position + new CPos(x, y, 0);
		}
	}
}
