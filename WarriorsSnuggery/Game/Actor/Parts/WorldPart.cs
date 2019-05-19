namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Basic information about the object.")]
	public class WorldPartInfo : PartInfo
	{
		[Desc("When true, the actor will show a damage text.")]
		public readonly bool ShowDamage;
		[Desc("When true, the actor will be able to trigger other objects.")]
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
