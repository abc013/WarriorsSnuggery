namespace WarriorsSnuggery.Physics
{
	public struct PhysicsLine
	{
		public readonly CPos Start;
		public readonly CPos End;

		public PhysicsLine(CPos start, CPos end)
		{
			Start = start;
			End = end;
		}
	}
}
