using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Objects
{
	public class ActorInit
	{
		public readonly uint ID;

		public readonly List<MiniTextNode> Nodes;
		public readonly ActorType Type;

		public readonly CPos Position;
		public readonly int Height;

		public ActorInit(uint id, List<MiniTextNode> nodes)
		{
			ID = id;
			Nodes = nodes;

			Type = Convert<ActorType>("Type", null);
			Position = Convert("Position", CPos.Zero);
			Height = Convert("Height", 0);
		}

		public ActorInit(uint id, MiniTextNode textNode)
		{
			ID = id;

			Position = textNode.Convert<CPos>();

			var node = new ActorNode(id, Position, textNode.Children);

			Type = node.Type;

			var list = new List<MiniTextNode>();
			var order = (short)(textNode.Order + 1);
			list.Add(new MiniTextNode("ActorInit", order, "Team", node.Team + ""));
			list.Add(new MiniTextNode("ActorInit", order, "Type", ActorCreator.GetName(node.Type)));
			if (node.IsBot)
			{
				var parent = new MiniTextNode("ActorInit", order, "BotPart", string.Empty);
				list.Add(parent);
				parent.Children.Add(new MiniTextNode("ActorInit", (short)(order + 1), "TargetPosition", node.BotTarget.ToString()));
			}
			if (node.IsPlayer)
				list.Add(new MiniTextNode("ActorInit", order, "PlayerPart", string.Empty));
			if (node.Health != 1f)
			{
				var parent = new MiniTextNode("ActorInit", order, "HealthPart", string.Empty);
				list.Add(parent);
				parent.Children.Add(new MiniTextNode("ActorInit", (short)(order + 1), "Health", node.Health.ToString(Settings.FloatFormat)));
			}
			if (node.IsPlayerSwitch)
			{
				var parent = new MiniTextNode("ActorInit", order, "PlayerSwitchPart", string.Empty);
				list.Add(parent);
				parent.Children.Add(new MiniTextNode("ActorInit", (short)(order + 1), "RelativeHP", node.RelativeHP.ToString(Settings.FloatFormat)));
				parent.Children.Add(new MiniTextNode("ActorInit", (short)(order + 1), "ActorType", ActorCreator.GetName(node.ToActor)));
				parent.Children.Add(new MiniTextNode("ActorInit", (short)(order + 1), "CurrentTick", node.Duration + ""));
			}

			Nodes = list;
		}

		public T Convert<T>(string rule, T @default)
		{
			var node = Nodes.FirstOrDefault(n => n.Key == rule);
			if (node != null)
				return node.Convert<T>();

			return @default;
		}

		class ActorNode
		{
			public readonly uint ID;
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

			public ActorNode(uint id, CPos position, List<MiniTextNode> nodes)
			{
				ID = id;
				Position = position;

				Loader.PartLoader.SetValues(this, nodes.ToArray());
			}
		}
	}
}
