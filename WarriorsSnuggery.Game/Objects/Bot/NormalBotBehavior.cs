using System;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Bot
{
	public class NormalBotBehavior : BotBehavior
	{
		int moral
		{
			get => moralVal;
			set => moralVal = Math.Clamp(value, -50, 50);
		}
		int moralVal;

		public NormalBotBehavior(World world, Actor self) : base(world, self) { }

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
