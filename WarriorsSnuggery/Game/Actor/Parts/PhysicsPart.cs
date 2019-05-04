namespace WarriorsSnuggery.Objects.Parts
{
	public class PhysicsPartInfo : PartInfo
	{
		public readonly CPos Size;
		public readonly Shape Shape;

		public override ActorPart Create(Actor self)
		{
			return new PhysicsPart(self, this);
		}

		public PhysicsPartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Shape":
						Shape = (Shape) node.ToEnum(typeof(Shape));

						break;
					case "Size":
						Size = node.ToCPos();

						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "PhysicsPart");
				}
			}
		}
	}

	public class PhysicsPart : ActorPart
	{
		readonly PhysicsPartInfo info;
		
		public Shape Shape
		{
			get { return info.Shape; }
			set { }
		}

		public int Height
		{
			get { return info.Size.Z; }
			set { }
		}

		public MPos Size
		{
			get { return new MPos(info.Size.X, info.Size.Y); }
			set { }
		}

		public PhysicsPart(Actor self, PhysicsPartInfo info) : base(self)
		{
			this.info = info;
		}
	}
}
