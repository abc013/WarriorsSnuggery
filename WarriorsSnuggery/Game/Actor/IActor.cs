namespace WarriorsSnuggery.Objects
{
	interface INoticeDamage
	{
		void OnDamage(Actor origin, int damage);
	}

	interface INoticeDeath
	{
		void OnDeath(Actor killer);
	}

	interface INoticeAttack
	{
		void OnAttack(CPos target);
	}

	interface INoticeManaAttack
	{
		void OnManaAttack();
	}

	interface INoticeMove
	{
		void OnMove(CPos old);
	}

	interface INoticeFailedMove
	{
		void OnFailedMove(CPos destination);
	}
}
