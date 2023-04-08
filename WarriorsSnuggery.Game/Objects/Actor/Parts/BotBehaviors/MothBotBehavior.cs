﻿using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Bot-Behavior type that aims to reproduce moth behavior.")]
	public class MothBotBehaviorPartInfo : BotBehaviorPartInfo
	{
		public MothBotBehaviorPartInfo(PartInitSet set) : base(set) { }
	}

	public class MothBotBehaviorPart : BotBehaviorPart
	{
		int tick;

		public MothBotBehaviorPart(Actor self, MothBotBehaviorPartInfo info) : base(self, info) {}

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (!HasGoodTarget)
			{
				if (tick++ % 20 == 0)
					Target = new Target(randomPosition());

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
