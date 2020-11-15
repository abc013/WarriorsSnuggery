using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Objects.Particles
{
	public static class ParticleCreator
	{
		public static readonly Dictionary<string, ParticleType> Types = new Dictionary<string, ParticleType>();

		public static void Load(string directory, string file)
		{
			var nodes = RuleReader.FromFile(directory, file);

			foreach (var node in nodes)
				Types.Add(node.Key, new ParticleType(node.Children));
		}

		public static string GetName(ParticleType type)
		{
			return Types.First(t => t.Value == type).Key;
		}

		public static Particle Create(World world, string name, CPos position, int height)
		{
			if (!Types.ContainsKey(name))
				throw new MissingInfoException(name);

			return Create(world, Types[name], position, height);
		}

		public static Particle Create(World world, ParticleType type, CPos position, int height)
		{
			return new Particle(world, position, height, type);
		}

		public static Particle Create(World world, ParticleInit init)
		{
			return new Particle(world, init);
		}
	}
}
