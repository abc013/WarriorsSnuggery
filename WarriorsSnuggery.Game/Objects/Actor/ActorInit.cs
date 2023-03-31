using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Objects.Actors
{
	public class ActorInit : TextNodeInitializer
	{
		public readonly uint ID;

		public readonly ActorType Type;

		public readonly CPos Position;

		public readonly byte Team;
		public readonly bool IsBot;
		public readonly bool IsPlayer;

		public readonly string ScriptTag;

		public ActorInit(uint id, ActorType type, CPos position, byte team, bool isBot, bool isPlayer, string scriptTag = "") : base(new List<TextNode>())
		{
			ID = id;
			Type = type;

			Position = position;

			Team = team;
			IsBot = isBot;
			IsPlayer = isPlayer;

			ScriptTag = scriptTag;
		}

		// MapFormat 1-current
		public ActorInit(uint id, List<TextNode> nodes, int mapFormat) : base(nodes)
		{
			ID = id;

			Type = Convert<ActorType>("Type", null);
			Position = Convert("Position", CPos.Zero);
			if (mapFormat < 2)
				Position = new CPos(Position.X, Position.Y, Convert("Height", 0));

			Team = Convert("Team", (byte)0);
			IsPlayer = Convert("IsPlayer", false);
			IsBot = Convert("IsBot", false);

			ScriptTag = Convert("ScriptTag", string.Empty);
		}

        public List<TextNodeInitializer> MakeInitializersWith(string rule)
        {
			var list = new List<TextNodeInitializer>();

			foreach (var node in Nodes.Where(n => n.Key == rule))
				list.Add(new TextNodeInitializer(node.Children));

            return list; 
        }

		public PartLoader MakeInitializerWith(ActorInit init, ActorPart part)
		{
			var type = part.GetType();
			var specification = part.Specification;

			// TODO: remove n.Key == type.Name, it is outdated from MapFormat 3.
			var parent = init.Nodes.FirstOrDefault(n => (n.Key == type.Name || n.Key == type.Name[..^4]) && (specification == null || specification == n.Specification));

			if (parent == null)
				return new PartLoader(new List<TextNode>());

			return new PartLoader(parent.Children);
		}
	}
}
