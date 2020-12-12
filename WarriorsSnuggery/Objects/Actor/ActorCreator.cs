using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Objects
{
	public static class ActorCreator
	{
		public static readonly Dictionary<string, ActorType> Types = new Dictionary<string, ActorType>();

		public static void Load(string directory, string file)
		{
			var actors = RuleReader.FromFile(directory, file);

			foreach (var actor in actors)
			{
				var name = actor.Key;

				var partinfos = new List<PartInfo>();

				var currentPartCounts = new Dictionary<string, int>();

				foreach (var child in actor.Children)
				{
					if (!currentPartCounts.ContainsKey(child.Key))
						currentPartCounts[child.Key] = 0;

					partinfos.Add(PartLoader.GetPart(currentPartCounts[child.Key]++, child.Key, child.Children));
				}

				Types.Add(name, new ActorType(partinfos.ToArray()));
			}
		}

		public static string GetName(ActorType type)
		{
			return Types.First(t => t.Value == type).Key;
		}

		public static Actor Create(World world, string name, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = 1f)
		{
			if (!Types.ContainsKey(name))
				throw new MissingInfoException(name);

			return Create(world, Types[name], position, team, isBot, isPlayer, health);
		}

		public static Actor Create(World world, ActorType type, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = 1f)
		{
			var init = new ActorInit(world.Game.NextActorID, type, position, 0, team, isBot, isPlayer);

			var actor = new Actor(world, init);
			if (actor.Health != null)
				actor.Health.RelativeHP = health;

			return actor;
		}

		public static Actor Create(World world, ActorInit init, bool overrideID, CPos offset)
		{
			Actor actor;
			if (overrideID)
				actor = new Actor(world, init, world.Game.NextActorID);
			else
				actor = new Actor(world, init);

			actor.Position += offset;

			return actor;
		}
	}
}
