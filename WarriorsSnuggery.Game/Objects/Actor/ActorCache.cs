using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Objects.Actors
{
	public static class ActorCache
	{
		public static readonly TypeDictionary<ActorType> Types = new TypeDictionary<ActorType>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
			{
				var name = node.Key;
				var parts = new PartInfo[node.Children.Count];

				var currentPartCounts = new Dictionary<string, int>();
				for (int i = 0; i < parts.Length; i++)
				{
					var child = node.Children[i];

					if (!currentPartCounts.ContainsKey(child.Key))
						currentPartCounts[child.Key] = 0;

					parts[i] = TypeLoader.GetPart(currentPartCounts[child.Key]++, child);
				}

				Types.Add(name, new ActorType(parts));
			}
		}

		public static Actor Create(World world, string name, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = -1f)
		{
			return Create(world, Types[name], position, team, isBot, isPlayer, health);
		}

		public static Actor Create(World world, ActorType type, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = -1f)
		{
			return new Actor(world, CreateInit(world, type, position, team, isBot, isPlayer, health));
		}

		public static Actor Create(World world, ActorInit init, bool overrideID)
		{
			return overrideID ? new Actor(world, init, world.Game.NextActorID) : new Actor(world, init);
		}

		public static ActorInit CreateInit(World world, string name, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = -1f)
		{
			return CreateInit(world, Types[name], position, team, isBot, isPlayer, health);
		}

		public static ActorInit CreateInit(World world, ActorType type, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = -1f)
		{
			return new ActorInit(world.Game.NextActorID, type, position, 0, team, isBot, isPlayer, health);
		}
	}
}
