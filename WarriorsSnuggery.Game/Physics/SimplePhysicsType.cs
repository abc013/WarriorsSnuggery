using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Physics
{
	public class SimplePhysicsType
	{
		[Desc("Shape of the physics.")]
		public readonly Shape Shape;

		[Desc("Physics radius width.")]
		public readonly int RadiusX;
		// only for box and drawing
		[Desc("Physics radius length.", "This will only be used for the box shape.")]
		public readonly int RadiusY;
		[Desc("Radius height of the Physics.")]
		public readonly int HeightRadius;

		[Desc("Offset based of the position of the object the physics are attached to.")]
		public readonly CPos Offset;
		[Desc("Height offset base of the height of the object the physics are attached to.")]
		public readonly int HeightOffset;

		public SimplePhysicsType(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);

			if (Shape == Shape.CIRCLE && RadiusX != RadiusY)
				throw new InvalidNodeException($"Physics with shape CIRCLE must have the same values for {nameof(RadiusX)} and {nameof(RadiusY)} (current: {RadiusX}, {RadiusY})");
		}

		public SimplePhysicsType(Shape shape, int radiusX, int radiusY, int heightRadius, CPos offset, int heightOffset = 0)
		{
			Shape = shape;
			RadiusX = radiusX;
			RadiusY = radiusY;
			HeightRadius = heightRadius;
			Offset = offset;
			HeightOffset = heightOffset;
		}
	}
}
