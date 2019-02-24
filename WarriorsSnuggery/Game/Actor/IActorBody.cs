namespace WarriorsSnuggery.Objects
{
	interface IActorBody
	{
		void Dispose();
		void NotifyAttack();
		void NotifyChange();
		void NotifyMove();
		void Tick();
	}
}