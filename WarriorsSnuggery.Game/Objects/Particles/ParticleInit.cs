using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Particles
{
	public sealed class ParticleInit : TextNodeInitializer
	{
		public readonly ParticleType Type;
		public readonly CPos Position;
		public readonly CPos InitialVelocity;

		public ParticleInit(ParticleType type, CPos position, CPos initialVelocity = default) : base(new List<TextNode>())
		{
			Type = type;
			Position = position;
			InitialVelocity = initialVelocity;
		}

		public ParticleInit(List<TextNode> nodes, int mapFormat) : base(nodes)
		{
			Type = Convert<ParticleType>("Type", null);
			Position = Convert("Position", CPos.Zero);
		}
	}
}
