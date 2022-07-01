using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Particles
{
	public sealed class ParticleInit
	{
		readonly bool fromSave;

		public readonly List<TextNode> Nodes;

		public readonly ParticleType Type;
		public readonly CPos Position;
		public readonly int Height;
		public readonly CPos InitialVelocity;

		public ParticleInit(ParticleType type, CPos position, int height, CPos initialVelocity)
		{
			Type = type;
			Position = position;
			Height = height;
			InitialVelocity = initialVelocity;

			// Empty list
			Nodes = new List<TextNode>();
		}

		public ParticleInit(List<TextNode> nodes)
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
