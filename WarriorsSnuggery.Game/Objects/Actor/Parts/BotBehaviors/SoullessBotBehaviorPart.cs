namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Bot-Behavior type that shows no self-initialized behavior at all. Used for testing purposes, serves no benefit otherwise.")]
	public class SoullessBotBehaviorPartInfo : BotBehaviorPartInfo
	{
		public SoullessBotBehaviorPartInfo(PartInitSet set) : base(set) { }
	}

	public class SoullessBotBehaviorPart : BotBehaviorPart
	{
		public SoullessBotBehaviorPart(Actor self, SoullessBotBehaviorPartInfo info) : base(self, info) { }

		public override void Tick() { }
	}
}
