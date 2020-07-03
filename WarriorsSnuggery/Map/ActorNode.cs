using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps
{
	public class ActorNode
	{
		public readonly int ID;
		public readonly CPos Position;

		public readonly ActorType Type;
		public readonly float Health = 1f;
		public readonly byte Team = Actor.NeutralTeam;
		public readonly bool IsBot = false;
		public readonly CPos BotTarget = new CPos(int.MaxValue, 0, 0);
		public readonly bool IsPlayer = false;

		public readonly bool IsPlayerSwitch = false;
		public readonly int Duration = 0;
		public readonly ActorType ToActor;
		public readonly float RelativeHP = 1f;

		public ActorNode(int id, CPos position, MiniTextNode[] nodes)
		{
			ID = id;
			Position = position;

			Loader.PartLoader.SetValues(this, nodes);
		}
	}
}