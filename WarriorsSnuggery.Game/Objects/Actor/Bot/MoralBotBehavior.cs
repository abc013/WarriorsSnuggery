using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors.Bot
{
	[Desc("Type uses a moral system to determine its strategy.")]
	public class MoralBotBehaviorType : BotBehaviorType
	{
		public MoralBotBehaviorType(List<TextNode> nodes) : base(nodes) { }

		public override BotBehavior Create(Actor self)
		{
			return new MoralBotBehavior(self, this);
		}
	}

	public class MoralBotBehavior : BotBehavior
	{
		readonly MoralBotBehaviorType type;

		int moral
		{
			get => moralVal;
			set => moralVal = Math.Clamp(value, -50, 50);
		}
		int moralVal;

		public MoralBotBehavior(Actor self, MoralBotBehaviorType type) : base(self)
		{
			this.type = type;
		}

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (!HasGoodTarget)
			{
				DefaultTickBehavior();
				return;
			}

			if (CanAttack)
				DefaultAttackBehavior();

			if (CanMove)
				DefaultMoveBehavior(moral < 0 ? 0.9f : 0.7f, moral < 0 ? 1.0f : 0.8f);

			moral++;
		}

		public override void OnDamage(Actor damager, int damage)
		{
			base.OnDamage(damager, damage);

			moral -= damage;
		}

		public override void OnKill(Actor killed)
		{
			base.OnKill(killed);
			moral += 10;
		}
	}
}
