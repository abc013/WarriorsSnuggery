using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Wall : PhysicsObject
	{
		public readonly WPos LayerPosition;
		readonly CPos renderPosition;
		public readonly WallType Type;
		readonly bool isHorizontal;

		public Wall(WPos position, WallType type) : base(position.ToCPos(), new WallRenderable(position.X % 2 != 0, type), type.Blocks ? new Physics.SimplePhysics(position.ToCPos() / new CPos(2, 1, 1), 0, position.X % 2 != 0 ? WarriorsSnuggery.Physics.Shape.LINE_HORIZONTAL : WarriorsSnuggery.Physics.Shape.LINE_VERTICAL, 512, 512, type.Height) : new Physics.SimplePhysics(position.ToCPos() / new CPos(2, 1, 1), 0, WarriorsSnuggery.Physics.Shape.NONE, 0, 0, 0))
		{
			LayerPosition = position;
			var pos = position.ToCPos() / new CPos(2, 1, 1);
			Position = pos + new CPos(0, -512, 0);
			isHorizontal = position.X % 2 != 0;
			if (isHorizontal)
			{
				Physics.Position += new CPos(0, 512, 0);
				renderPosition = pos + new CPos(-512, -1536, 0);
			}
			else
			{
				Position += new CPos(0, 0, 2048);
				Physics.Position += new CPos(0, 1024, 0);
				renderPosition = pos + new CPos(-83, -512, 0);
			}
			Type = type;
		}

		public override void Render()
		{
			Renderable.SetPosition(renderPosition);
			base.Render();
		}

		public override void Dispose()
		{
			base.Dispose();
		}

		public override void CheckVisibility()
		{
			Renderable.Visible = VisibilitySolver.IsVisibleIgnoringBounds(new WPos(LayerPosition.X / 2, LayerPosition.Y - 1, 0));

			if (!isHorizontal)
				Renderable.Visible |= VisibilitySolver.IsVisibleIgnoringBounds(new WPos(LayerPosition.X / 2, LayerPosition.Y, 0));
		}
	}
}
