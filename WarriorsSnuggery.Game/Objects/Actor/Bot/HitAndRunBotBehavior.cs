using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors.Bot
{
	[Desc("Type that aims to reproduce a \"hit and run\" technique.")]
	public class HitAndRunBotBehaviorType : BotBehaviorType
	{
		public HitAndRunBotBehaviorType(List<TextNode> nodes) : base(nodes) { }

		public override BotBehavior Create(Actor self)
		{
			return new HitAndRunBotBehavior(self, this);
		}
	}

	public class HitAndRunBotBehavior : BotBehavior
	{
		readonly HitAndRunBotBehaviorType type;

		bool hide;
		int hideDuration;

		public HitAndRunBotBehavior(Actor self, HitAndRunBotBehaviorType type) : base(self)
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

			if (hide && hideDuration-- <= 0 && (Self.Weapon == null || Self.Weapon.ReloadDone))
				hide = false;
			else if (CanAttack)
				DefaultAttackBehavior();

			if (CanMove)
			{
				if (hide)
					DefaultMoveBehavior(8f, 9f);
				else
					DefaultMoveBehavior();
			}
		}

		public override void OnDamage(Actor damager, int damage)
		{
			base.OnDamage(damager, damage);

			hideDuration += damage;
			hide = true;
		}

		public override void OnKill(Actor killer)
		{
			hideDuration -= 10;
		}
	}
}

