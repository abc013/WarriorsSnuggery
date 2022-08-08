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
		public readonly CPos InitialVelocity;

		public ParticleInit(ParticleType type, CPos position, CPos initialVelocity = default)
		{
			Type = type;
			Position = position;
			InitialVelocity = initialVelocity;

			// Empty list
			Nodes = new List<TextNode>();
		}

		public ParticleInit(List<TextNode> nodes, int mapFormat)
		{
			fromSave = true;

			Nodes = nodes;

			Type = Convert<ParticleType>("Type", null);
			Position = Convert("Position", CPos.Zero);
			if (mapFormat < 2)
				Position = new CPos(Position.X, Position.Y, Convert("Height", 0));
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
