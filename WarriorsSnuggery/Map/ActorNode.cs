namespace WarriorsSnuggery.Maps
{
	public class ActorNode
	{
		public readonly int ID;
		public readonly CPos Position;

		public readonly string Type = "";
		public readonly float Health = 1f;
		public readonly int Team = Objects.Actor.NeutralTeam;
		public readonly bool IsBot = false;
		public readonly bool IsPlayer = false;

		public ActorNode(int id, CPos position, MiniTextNode[] nodes)
		{
			ID = id;
			Position = position;

			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Type":
						Type = node.Value;

						break;
					case "Team":
						Team = node.ToInt();

						break;
					case "Health":
						Health = node.ToFloat();

						break;
					case "IsBot":
						IsBot = node.ToBoolean();

						break;
					case "IsPlayer":
						IsPlayer = node.ToBoolean();

						break;
				}
			}
		}

		public ActorNode(int id, CPos position, string type, int team, float health, bool isBot)
		{
			ID = id;
			Position = position;
			Type = type;
			Team = team;
			Health = health;
			IsBot = isBot;
		}
	}
}