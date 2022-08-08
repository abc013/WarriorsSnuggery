using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Physics
{
	public class SimplePhysicsType
	{
		[Desc("Shape of the physics.")]
		public readonly Shape Shape;

		[Desc("Physics radius boundaries.", "For LINE shape: either X or Y has to be 0.", "For CIRCLE shape: X and Y must be equal.")]
		public readonly CPos Boundaries;

		[Desc("Offset based of the position of the object the physics are attached to.", "Z-Coordinate is used for height.")]
		public readonly CPos Offset;

		public SimplePhysicsType(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);

			if (Shape == Shape.LINE && (Boundaries.X != 0 || Boundaries.Y != 0))
				throw new InvalidNodeException($"Physics with shape LINE must have at least one dimension set to zero (current: {Boundaries.X}, {Boundaries.Y})");

			if (Shape == Shape.CIRCLE && Boundaries.X != Boundaries.Y)
				throw new InvalidNodeException($"Physics with shape CIRCLE must have the same values for X and Y dimensions (current: {Boundaries.X}, {Boundaries.Y})");
		}

		public SimplePhysicsType(Shape shape, CPos boundaries, CPos offset)
		{
			Shape = shape;
			Boundaries = boundaries;
			Offset = offset;
		}
	}
}
