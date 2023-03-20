namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("This part enables actors to have armor.")]
	public class ArmorPartInfo : PartInfo
	{
		[Require, Desc("Name of the armor.")]
		public readonly string Name;

		public ArmorPartInfo(PartInitSet set) : base(set) { }
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
