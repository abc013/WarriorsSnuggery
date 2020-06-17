using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Objects
{
	public static class ActorCreator
	{
		public static readonly Dictionary<string, ActorType> Types = new Dictionary<string, ActorType>();

		public static void Load(string directory, string file)
		{
			var actors = RuleReader.Read(directory, file);

			foreach (var actor in actors)
			{
				var name = actor.Key;

				var partinfos = new List<PartInfo>();

				foreach (var child in actor.Children)
					partinfos.Add(Loader.PartLoader.GetPart(child.Key, child.Children.ToArray()));

				Types.Add(name, new ActorType(partinfos.ToArray()));
			}
		}

		public static string GetName(ActorType type)
		{
			return Types.FirstOrDefault(t => t.Value == type).Key;
		}

		public static Actor Create(World world, string name, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = 1f)
		{
			if (!Types.ContainsKey(name))
				throw new MissingInfoException(name);

			return Create(world, Types[name], position, team, isBot, isPlayer, health);
		}

		public static Actor Create(World world, ActorType type, CPos position, byte team = 0, bool isBot = false, bool isPlayer = false, float health = 1f)
		{
			var actor = new Actor(world, type, position, team, isBot, isPlayer);
			if (actor.Health != null)
				actor.Health.RelativeHP = health;

			return actor;
		}
	}
}
