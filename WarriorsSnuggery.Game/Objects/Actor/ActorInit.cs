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
		public readonly int Height;

		public readonly byte Team;
		public readonly bool IsBot;
		public readonly bool IsPlayer;

		// HACK: save health here temporarily.
		public readonly float Health = -1f;

		public ActorInit(uint id, ActorType type, CPos position, int height, byte team, bool isBot, bool isPlayer, float health = -1f)
		{
			ID = id;
			Type = type;

			Position = position;
			Height = height;

			Team = team;
			IsBot = isBot;
			IsPlayer = isPlayer;

			Health = health;

			// Empty list
			Nodes = new List<TextNode>();
		}

		public ActorInit(uint id, List<TextNode> nodes)
		{
			ID = id;
			Nodes = nodes;

			Type = Convert<ActorType>("Type", null);
			Position = Convert("Position", CPos.Zero);
			Height = Convert("Height", 0);

			Team = Convert("Team", (byte)0);
			IsPlayer = Nodes.Any(n => n.Key == "PlayerPart");
			IsBot = Nodes.Any(n => n.Key == "BotPart");
		}

		public ActorInit(uint id, TextNode textNode)
		{
			ID = id;

			Position = textNode.Convert<CPos>();

			var node = new ActorNode(id, Position, textNode.Children);

			Type = node.Type;

			Team = node.Team;
			IsPlayer = node.IsPlayer;
			IsBot = node.IsBot;

			var list = new List<TextNode>();
			var order = (short)(textNode.Order + 1);
			list.Add(new TextNode("ActorInit", order, "Team", node.Team));
			list.Add(new TextNode("ActorInit", order, "Type", ActorCache.Types[node.Type]));
			if (node.IsBot)
			{
				var parent = new TextNode("ActorInit", order, "BotPart", string.Empty);
				list.Add(parent);
				parent.Children.Add(new TextNode("ActorInit", (short)(order + 1), "TargetPosition", node.BotTarget));
			}
			if (node.IsPlayer)
				list.Add(new TextNode("ActorInit", order, "PlayerPart", string.Empty));
			if (node.Health != 1f)
			{
				if (Type.PartInfos.FirstOrDefault(p => p is Parts.HealthPartInfo) is Parts.HealthPartInfo health)
				{
					var parent = new TextNode("ActorInit", order, "HealthPart", string.Empty);
					list.Add(parent);
					parent.Children.Add(new TextNode("ActorInit", (short)(order + 1), "Health", node.Health * health.MaxHealth));
				}
			}
			if (node.IsPlayerSwitch)
			{
				var parent = new TextNode("ActorInit", order, "PlayerSwitchPart", string.Empty);
				list.Add(parent);
				parent.Children.Add(new TextNode("ActorInit", (short)(order + 1), "RelativeHP", node.RelativeHP));
				parent.Children.Add(new TextNode("ActorInit", (short)(order + 1), "ActorType", ActorCache.Types[node.ToActor]));
				parent.Children.Add(new TextNode("ActorInit", (short)(order + 1), "CurrentTick", node.Duration));
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

#pragma warning disable CS0649
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
#pragma warning restore CS0649

			public ActorNode(uint id, CPos position, List<TextNode> nodes)
			{
				ID = id;
				Position = position;

				TypeLoader.SetValues(this, nodes);
			}
		}
	}
}
