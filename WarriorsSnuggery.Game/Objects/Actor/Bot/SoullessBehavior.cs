using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors.Bot
{
	[Desc("Type that shows no self-initialized behavior at all.")]
	public class SoullessBotBehaviorType : BotBehaviorType
	{
		public SoullessBotBehaviorType(List<TextNode> nodes) : base(nodes) { }

		public override BotBehavior Create(Actor self)
		{
			return new SoullessBotBehavior(self, this);
		}
	}

	public class SoullessBotBehavior : BotBehavior
	{
		public SoullessBotBehavior(Actor self, SoullessBotBehaviorType type) : base(self) { }

		public override void Tick()
		{
			if (!CanMove)
				return;

			DefaultMoveBehavior(0, 0.1f);
		}
	}
}
