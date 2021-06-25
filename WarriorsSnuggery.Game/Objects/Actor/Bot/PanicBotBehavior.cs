using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Actors.Bot
{
	[Desc("Type that aims to reproduce moth behavior.")]
	public class PanicBotBehaviorType : BotBehaviorType
	{
		public PanicBotBehaviorType(List<TextNode> nodes) : base(nodes) { }

		public override BotBehavior Create(Actor self)
		{
			return new PanicBotBehavior(self, this);
		}
	}

	public class PanicBotBehavior : BotBehavior
	{
		readonly PanicBotBehaviorType type;

		int panic;
		bool inPanic;
		float angle;

		public PanicBotBehavior(Actor self, PanicBotBehaviorType type) : base(self)
		{
			this.type = type;
		}

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (Self.IsAlive && panic > Self.Health.HP * 2)
				inPanic = true;

			if (inPanic)
			{
				if (panic-- <= 0)
					inPanic = false;

				if (panic % 20 == 0)
					angle = (float)Self.World.Game.SharedRandom.NextDouble();

				if (CanMove && DistToTarget > 512)
					Self.AccelerateSelf(angle);

				if (!HasGoodTarget && Self.World.Game.SharedRandom.Next(100) == 0)
					PredictiveAttack(new Target(randomPosition(), 0));
				else
					PredictiveAttack(new Target((Target.Position + randomPosition()) / new CPos(2, 2, 2), Target.Height));
			}
			else
			{
				if (!HasGoodTarget)
				{
					if (Self.IsAlive && panic <= Self.Health.HP * 1.8f)
						panic++;

					DefaultTickBehavior();
					return;
				}
				panic--;

				if (CanAttack)
					DefaultAttackBehavior();

				if (CanMove)
					DefaultMoveBehavior();
			}
		}

		CPos randomPosition(int range = 5120)
		{
			var x = Self.World.Game.SharedRandom.Next(range);
			var y = Self.World.Game.SharedRandom.Next(range);

			return Self.Position + new CPos(x, y, 0);
		}

		public override void OnDamage(Actor damager, int damage)
		{
			base.OnDamage(damager, damage);

			panic += damage * 2;
		}

		public override void OnKill(Actor killer)
		{
			panic = 0;
		}
	}
}
