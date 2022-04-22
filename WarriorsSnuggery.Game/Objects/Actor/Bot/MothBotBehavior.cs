using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Actors.Bot
{
	[Desc("Type that aims to reproduce moth behavior.")]
	public class MothBotBehaviorType : BotBehaviorType
	{
		public MothBotBehaviorType(List<TextNode> nodes) : base(nodes) { }

		public override BotBehavior Create(Actor self)
		{
			return new MothBotBehavior(self, this);
		}
	}

	public class MothBotBehavior : BotBehavior
	{
		readonly MothBotBehaviorType type;

		int tick;

		public MothBotBehavior(Actor self, MothBotBehaviorType type) : base(self)
		{
			this.type = type;
		}

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (!HasGoodTarget)
			{
				if (tick++ % 20 == 0)
					Target = new Target(randomPosition(), 0);

				DefaultTickBehavior();
				return;
			}

			DefaultAttackBehavior();
			DefaultMoveBehavior();
		}

		CPos randomPosition()
		{
			var x = Program.SharedRandom.Next(2048) - 1024;
			var y = Program.SharedRandom.Next(2048) - 1024;

			return Self.Position + new CPos(x, y, 0);
		}

		public override void OnKill(Actor killer) { }
	}
}
