using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Particles
{
	public static class ParticleCache
	{
		public static readonly TypeDictionary<ParticleType> Types = new TypeDictionary<ParticleType>();

		public static void Load(List<TextNode> nodes)
		{
			foreach (var node in nodes)
				Types.Add(node.Key, new ParticleType(node.Children));
		}

		public static Particle Create(World world, string name, CPos position, CPos initialVelocity = default)
		{
			return Create(world, Types[name], position, initialVelocity);
		}

		public static Particle Create(World world, ParticleType type, CPos position, CPos initialVelocity = default)
		{
			var init = new ParticleInit(type, position, initialVelocity);

			return new Particle(world, init);
		}

		public static Particle Create(World world, ParticleInit init)
		{
			return new Particle(world, init);
		}
	}
}
