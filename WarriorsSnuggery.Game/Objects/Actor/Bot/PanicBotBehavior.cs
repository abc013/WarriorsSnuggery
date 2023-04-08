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
		int panicBuildup;
		int panicDuration;
		float panicAngle;

		public PanicBotBehavior(Actor self, PanicBotBehaviorType type) : base(self)
		{

		}

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (panicBuildup > 0)
				panicBuildup--;

			if (panicDuration-- > 0)
			{
				if (panicDuration % 20 == 0)
					panicAngle = Angle.Cast(Self.World.Game.SharedRandom.Next(360));

				Self.AccelerateSelf(panicAngle);

				if (Self.World.Game.SharedRandom.Next(100) < 10)
					BotUtils.PredictiveAttack(Self, randomTarget());
				else if (HasGoodTarget)
					BotUtils.PredictiveAttack(Self, Target);
			}
			else
			{
				if (!HasGoodTarget)
				{
					DefaultTickBehavior();
					return;
				}

				DefaultAttackBehavior();
				DefaultMoveBehavior();
			}
		}

		Target randomTarget(int range = 5120)
		{
			var ranAngle = Angle.Cast(Self.World.Game.SharedRandom.Next(360));
			var ranLength = Self.World.Game.SharedRandom.Next(range);

			return new Target(Self.Position + CPos.FromFlatAngle(ranAngle, ranLength));
		}

		public override void OnDamage(Actor damager, int damage)
		{
			base.OnDamage(damager, damage);

			panicBuildup += damage * 8;
			if (Self.IsAlive && panicBuildup > Self.Health.HP && Self.Health.RelativeHP < 0.5)
			{
				panicBuildup = 0;
				panicDuration += 600;
			}
		}

		public override void OnKill(Actor killer)
		{
			panicBuildup = 0;
		}
	}
}
