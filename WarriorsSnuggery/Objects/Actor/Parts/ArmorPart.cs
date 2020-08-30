namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("This part enables actors to have armor. .")]
	public class ArmorPartInfo : PartInfo
	{
		[Desc("Name of the armor.")]
		public readonly string Name;

		public override ActorPart Create(Actor self)
		{
			return new ArmorPart(self, this);
		}

		public ArmorPartInfo(string internalName, MiniTextNode[] nodes) : base(internalName, nodes) { }
	}

	public class ArmorPart : ActorPart
	{
		readonly ArmorPartInfo info;

		public string Name => info.Name;

		public ArmorPart(Actor self, ArmorPartInfo info) : base(self)
		{
			this.info = info;
		}
	}
}
