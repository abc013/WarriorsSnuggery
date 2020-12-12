using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Objects.Particles
{
	public sealed class ParticleInit
	{
		readonly bool fromSave;

		public readonly List<MiniTextNode> Nodes;

		public readonly ParticleType Type;
		public readonly CPos Position;
		public readonly int Height;

		public ParticleInit(ParticleType type, CPos position, int height)
		{
			Type = type;
			Position = position;
			Height = height;

			// Empty list
			Nodes = new List<MiniTextNode>();
		}

		public ParticleInit(List<MiniTextNode> nodes)
		{
			fromSave = true;

			Nodes = nodes;

			Type = Convert<ParticleType>("Type", null);
			Position = Convert("Position", CPos.Zero);
			Height = Convert("Height", 0);
		}

		public T Convert<T>(string rule, T @default)
		{
			if (!fromSave)
				return @default;

			var node = Nodes.FirstOrDefault(n => n.Key == rule);
			if (node != null)
				return node.Convert<T>();

			return @default;
		}
	}
}
