using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Objects.Particles
{
	public sealed class ParticleInit
	{
		public readonly List<MiniTextNode> Nodes;

		public readonly ParticleType Type;
		public readonly CPos Position;
		public readonly int Height;

		public ParticleInit(List<MiniTextNode> nodes)
		{
			Nodes = nodes;

			Type = Convert<ParticleType>("Type", null);
			Position = Convert("Position", CPos.Zero);
			Height = Convert("Height", 0);
		}

		public T Convert<T>(string rule, T @default)
		{
			var node = Nodes.FirstOrDefault(n => n.Key == rule);
			if (node != null)
				return node.Convert<T>();

			return @default;
		}
	}
}
