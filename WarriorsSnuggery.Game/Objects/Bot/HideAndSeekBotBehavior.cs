using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Bot
{
	public class HideAndSeekBotBehavior : BotBehavior
	{
		bool hide;
		int hideDuration;

		public HideAndSeekBotBehavior(World world, Actor self) : base(world, self) { }

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

