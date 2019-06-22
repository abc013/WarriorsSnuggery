namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Attach this to an actor so it can collide with other actors.")]
	public class PhysicsPartInfo : PartInfo
	{
		[Desc("Size of the collision field.", "Z is used for height boundary.")]
		public readonly CPos Size;
		[Desc("Shape of the collision field.", "Possible: CIRCLE, RECTANGLE, LINE_HORIZONTAL, LINE_VERTICAL, NONE")]
		public readonly Shape Shape;

		public override ActorPart Create(Actor self)
		{
			return new PhysicsPart(self, this);
		}

		public PhysicsPartInfo(MiniTextNode[] nodes) : base(nodes)
		{

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
