namespace WarriorsSnuggery.Objects
{
	// TODO: Add PREPARE_MOVE and END_MOVE
	public enum ActionType
	{
		IDLE,
		MOVE,
		PREPARE_ATTACK,
		ATTACK,
		END_ATTACK
	}

	public class ActorAction
	{
		public static readonly ActorAction Default = new ActorAction(ActionType.IDLE, true, true);

		public readonly ActionType Type;
		public readonly bool Skippable;
		public readonly bool Infinite;
		public int CurrentTick { get; private set; }

		public bool ActionOver => CurrentTick <= 0;

		public ActorAction(ActionType type, bool skippable, bool infinite = false)
		{
			Type = type;
			Skippable = skippable;
			Infinite = infinite;
		}

		public void ExtendAction(int ticks)
		{
			CurrentTick += ticks;
		}

		public bool Tick()
		{
			return !Infinite && CurrentTick-- < 0;
		}
	}
}
