using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Objects.Particles
{
	public static class ParticleCreator
	{
		public static readonly Dictionary<string, ParticleType> Types = new Dictionary<string, ParticleType>();

		public static void Load(string directory, string file)
		{
			var nodes = RuleReader.Read(directory, file);

			foreach (var node in nodes)
				Types.Add(node.Key, new ParticleType(node.Children.ToArray()));
		}

		public static Particle Create(World world, string name, CPos position, int height, Random random)
		{
			if (!Types.ContainsKey(name))
				throw new MissingInfoException(name);

			return Create(world, Types[name], position, height, random);
		}

		public static Particle Create(World world, ParticleType type, CPos position, int height, Random random)
		{
			return new Particle(world, position, height, type, random);
		}
	}
}
