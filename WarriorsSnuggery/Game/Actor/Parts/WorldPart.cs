namespace WarriorsSnuggery.Objects.Parts
{
	public class WorldPartInfo : PartInfo
	{
		public readonly bool ShowDamage;
		public readonly bool CanTrigger;

		public override ActorPart Create(Actor self)
		{
			return new WorldPart(self, this);
		}

		public WorldPartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "ShowDamage":
						ShowDamage = node.ToBoolean();

						break;
					case "CanTrigger":
						CanTrigger = node.ToBoolean();

						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "WorldPart");
				}
			}
		}
	}

	public class WorldPart : ActorPart
	{
		readonly WorldPartInfo info;

		public bool ShowDamage
		{
			get { return info.ShowDamage; }
			set { }
		}

		public bool CanTrigger
		{
			get { return info.CanTrigger; }
			set { }
		}

		public WorldPart(Actor self, WorldPartInfo info) : base(self)
		{
			this.info = info;
		}
	}
}
