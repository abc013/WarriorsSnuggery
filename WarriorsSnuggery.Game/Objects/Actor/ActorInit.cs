using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors
{
	public class ActorInit
	{
		public readonly uint ID;

		public readonly List<TextNode> Nodes;
		public readonly ActorType Type;

		public readonly CPos Position;

		public readonly byte Team;
		public readonly bool IsBot;
		public readonly bool IsPlayer;

		public readonly string ScriptTag;

		public ActorInit(uint id, ActorType type, CPos position, byte team, bool isBot, bool isPlayer, string scriptTag = "")
		{
			ID = id;
			Type = type;

			Position = position;

			Team = team;
			IsBot = isBot;
			IsPlayer = isPlayer;

			ScriptTag = scriptTag;

			// Empty list
			Nodes = new List<TextNode>();
		}

		// MapFormat 1-current
		public ActorInit(uint id, List<TextNode> nodes, int mapFormat)
		{
			ID = id;
			Nodes = nodes;

			Type = Convert<ActorType>("Type", null);
			Position = Convert("Position", CPos.Zero);
			if (mapFormat < 2)
				Position = new CPos(Position.X, Position.Y, Convert("Height", 0));

			Team = Convert("Team", (byte)0);
			IsPlayer = Convert("IsPlayer", false);
			IsBot = Convert("IsBot", false);

			ScriptTag = Convert("ScriptTag", string.Empty);
		}

		public T Convert<T>(string rule, T @default)
		{
			var node = Nodes.FirstOrDefault(n => n.Key == rule);
			if (node != null)
				return node.Convert<T>();

			return @default;
		}
	}
}
