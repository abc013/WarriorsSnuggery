namespace WarriorsSnuggery.Objects.Parts
{
	public class SpawnOnMovePartInfo : PartInfo
	{
		public readonly float Probability = 1f;
		public readonly int Count;
		public readonly int Tick;
		public readonly string Name;
		public readonly bool InheritsTeam;
		public readonly bool InheritsBot;
		public readonly string Type;
		public readonly string Condition;
		public readonly CPos Offset;

		public override ActorPart Create(Actor self)
		{
			return new SpawnOnMovePart(self, this);
		}

		public SpawnOnMovePartInfo(MiniTextNode[] nodes) : base(nodes)
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
					case "Tick":
						Tick = node.ToInt();
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
					case "Offset":
						Offset = node.ToCPos();
						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "SpawnOnIdlePart");
				}
			}
		}
	}

	/// <summary>
	/// Spawns objects while the player is moving.
	/// </summary>
	public class SpawnOnMovePart : ActorPart
	{
		readonly SpawnOnMovePartInfo info;
		int curTick;

		public SpawnOnMovePart(Actor self, SpawnOnMovePartInfo info) : base(self)
		{
			this.info = info;
		}

		public override void Tick()
		{
			if (self.CurrentAction == ActorAction.MOVING && curTick-- < 0)
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

			PhysicsObject @object = null;
			switch (info.Type)
			{
				case "ACTOR":
					@object = ActorCreator.Create(self.World, info.Name, randomPosition(), info.InheritsTeam ? self.Team : Actor.NeutralTeam, info.InheritsBot ? self.IsBot : false);
					break;
				case "PARTICLE":
					@object = ParticleCreator.Create(info.Name, randomPosition());
					break;
				case "WEAPON":
					@object = WeaponCreator.Create(self.World, info.Name, randomPosition(), randomPosition());
					break;
				default:
					throw new YamlInvalidNodeException(string.Format("SpawnOnIdle does not create objects of class '{0}'.", info.Type));
			}
			self.World.Add(@object);
		}

		CPos randomPosition()
		{
			var size = self.Physics != null ? self.Physics.Radius : 40;
			var x = Program.SharedRandom.Next(size) - size / 2;
			var y = Program.SharedRandom.Next(size) - size / 2;
			return self.Position + new CPos(x, y, 0) + info.Offset;
		}
	}
}
