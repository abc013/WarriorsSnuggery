﻿namespace WarriorsSnuggery.Maps
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

			Loader.PartLoader.SetValues(this, nodes);
		}
	}
}