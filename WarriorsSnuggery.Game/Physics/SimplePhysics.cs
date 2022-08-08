using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps.Layers;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Physics
{
	public sealed class SimplePhysics
	{
		public static readonly SimplePhysics Empty = new SimplePhysics(null, new SimplePhysicsType(Shape.NONE, CPos.Zero, CPos.Zero));

		readonly SimplePhysicsType type;

		readonly PositionableObject positionable;

		public PhysicsSector[] Sectors = new PhysicsSector[0];

		public CPos Position => positionable.Position + new CPos(type.Offset.X, type.Offset.Y, 0);
		public int Height => positionable.Position.Z + type.Offset.Z;

		public bool IsEmpty => type.Shape == Shape.NONE;

		public CPos Boundaries => type.Boundaries;
		public Shape Shape => type.Shape;

		public SimplePhysics(PositionableObject positionable, SimplePhysicsType type)
		{
			this.positionable = positionable;
			this.type = type;
		}

		public (CPos start, CPos end)[] GetLines()
		{
			return Shape switch
			{
				Shape.LINE => new (CPos, CPos)[]
				{
					(Position - new CPos(Boundaries.X, Boundaries.Y, 0), Position + new CPos(Boundaries.X, Boundaries.Y, 0))
				},
				Shape.RECTANGLE => new (CPos, CPos)[]
				{
					(Position - new CPos(Boundaries.X,  Boundaries.Y, 0), Position + new CPos(-Boundaries.X, Boundaries.Y, 0)),
					(Position - new CPos(Boundaries.X,  Boundaries.Y, 0), Position + new CPos(Boundaries.X, -Boundaries.Y, 0)),
					(Position + new CPos(-Boundaries.X, Boundaries.Y, 0), Position + new CPos(Boundaries.X,  Boundaries.Y, 0)),
					(Position + new CPos(Boundaries.X, -Boundaries.Y, 0), Position + new CPos(Boundaries.X,  Boundaries.Y, 0))
				},
				_ => new (CPos, CPos)[0],
			};
		}

		public void RenderDebug()
		{
			if (IsEmpty)
				return;

			switch (Shape)
			{
				case Shape.CIRCLE:
					ColorManager.DrawCircle(Position, Boundaries.X / 1024f, Color.Magenta);
					break;
				case Shape.RECTANGLE:
					ColorManager.DrawLineQuad(Position, new CPos(Boundaries.X, Boundaries.Y, 0), Color.Magenta);
					break;
				case Shape.LINE:
					ColorManager.DrawLine(Position - new CPos(Boundaries.X, Boundaries.Y, 0), Position + new CPos(Boundaries.X, Boundaries.Y, 0), Color.Magenta);
					break;
			}
		}

		public void RemoveSectors()
		{
			if (IsEmpty)
				return;

			foreach (var sector in Sectors)
				sector.Remove(this);
		}
	}
}
