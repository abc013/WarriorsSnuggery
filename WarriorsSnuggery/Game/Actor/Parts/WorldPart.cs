namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Basic information about the object.")]
	public class WorldPartInfo : PartInfo
	{
		[Desc("When true, the actor will show a damage text.")]
		public readonly bool ShowDamage;

		[Desc("When true, the actor will be able to trigger other objects.")]
		public readonly bool CanTrigger;

		[Desc("Height of the actor.")]
		public readonly int Height;

		[Desc("Hovering of the actor.", "This will create a hover-effect for flying actors.")]
		public readonly int Hover;

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
					case "Height":
						Height = node.ToInt();

						break;
					case "Hover":
						Hover = node.ToInt();

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

		public int Height
		{
			get { return info.Height; }
			set { }
		}

		public int Hover
		{
			get { return info.Hover; }
			set { }
		}

		public WorldPart(Actor self, WorldPartInfo info) : base(self)
		{
			this.info = info;
		}
	}
}
