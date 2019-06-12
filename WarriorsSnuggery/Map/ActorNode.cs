namespace WarriorsSnuggery.Maps
{
	public class ActorNode
	{
		public readonly int ID;
		public readonly CPos Position;

		public readonly string Type = "";
		public readonly float Health = 1f;
		public readonly byte Team = Objects.Actor.NeutralTeam;
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
						var pre = node.ToInt();
						if (pre >= Settings.MaxTeams || pre < 0)
							throw new YamlInvalidNodeException(string.Format("Actors can not be assigned to a negative team or a team value greater than {0}.", Settings.MaxTeams - 1));

						Team = (byte)pre;
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

		public ActorNode(int id, CPos position, string type, byte team, float health, bool isBot)
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