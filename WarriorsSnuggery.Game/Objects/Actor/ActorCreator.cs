using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Objects.Actors
{
	public static class ActorCreator
	{
		public static readonly Dictionary<string, ActorType> Types = new Dictionary<string, ActorType>();

		public static void Load(string directory, string file)
		{
			var actors = TextNodeLoader.FromFile(directory, file);

			foreach (var actor in actors)
			{
				var name = actor.Key;
				var parts = new PartInfo[actor.Children.Count];

				var currentPartCounts = new Dictionary<string, int>();
				for (int i = 0; i < parts.Length; i++)
				{
					var child = actor.Children[i];

					if (!currentPartCounts.ContainsKey(child.Key))
						currentPartCounts[child.Key] = 0;

					parts[i] = TypeLoader.GetPart(currentPartCounts[child.Key]++, child);
				}

				Types.Add(name, new ActorType(parts));
			}
		}

		public static string GetName(ActorType type)
		{
			return Types.First(t => t.Value == type).Key;
		}

		public static Actor Create(World world, string name, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = -1f)
		{
			if (!Types.ContainsKey(name))
				throw new MissingInfoException(name);

			return Create(world, Types[name], position, team, isBot, isPlayer, health);
		}

		public static Actor Create(World world, ActorType type, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = -1f)
		{
			return new Actor(world, CreateInit(world, type, position, team, isBot, isPlayer, health));
		}

		public static Actor Create(World world, ActorInit init, bool overrideID, CPos offset)
		{
			var actor = overrideID ? new Actor(world, init, world.Game.NextActorID) : new Actor(world, init);

			actor.Position += offset;
			return actor;
		}

		public static ActorInit CreateInit(World world, string name, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = -1f)
		{
			if (!Types.ContainsKey(name))
				throw new MissingInfoException(name);

			return CreateInit(world, Types[name], position, team, isBot, isPlayer, health);
		}

		public static ActorInit CreateInit(World world, ActorType type, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = -1f)
		{
			return new ActorInit(world.Game.NextActorID, type, position, 0, team, isBot, isPlayer, health);
		}
	}
}
