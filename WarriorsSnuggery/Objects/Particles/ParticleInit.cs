using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Particles
{
	public sealed class ParticleInit
	{
		public readonly string File;

		public readonly List<MiniTextNode> Nodes;
		public readonly ParticleType Type;

		public readonly CPos Position;
		public readonly int Height;

		public ParticleInit(string file, List<MiniTextNode> nodes)
		{
			File = file;
			Nodes = nodes;

			Type = Convert<ParticleType>("Type", null);
			Position = Convert("Position", CPos.Zero);
			Height = Convert("Height", 0);
		}

		public T Convert<T>(string rule, T @default)
		{
			var node = Nodes.FirstOrDefault(n => n.Key == rule);
			if (node != null)
				return NodeConverter.Convert<T>(File, node);

			return @default;
		}
	}
}
