namespace WarriorsSnuggery.Objects.Actors
{
	public enum ActionType
	{
		IDLE = 0,
		PREPARE_MOVE = 1,
		MOVE = 2,
		END_MOVE = 4,
		PREPARE_ATTACK = 8,
		ATTACK = 16,
		END_ATTACK = 32,
		DESPAWN = 64,
		SPAWN = 128
	}
}
