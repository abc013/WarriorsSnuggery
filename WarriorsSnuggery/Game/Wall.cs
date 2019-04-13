/*
 * User: Andreas
 * Date: 01.06.2018
 * Time: 21:02
 */
using System;

namespace WarriorsSnuggery.Objects
{
	public class Wall : PhysicsObject
	{
		public readonly WPos LayerPosition;
		readonly CPos renderPosition;
		public readonly WallType Type;

		public Wall(WPos position, WallType type) : base(position.ToCPos(), new WallRenderable(position.X % 2 != 0, type), type.Blocks ? new Physics(position.ToCPos() / new CPos(2,1,1), 0, position.X % 2 != 0 ? Shape.LINE_HORIZONTAL : Shape.LINE_VERTICAL, 512, type.Height) : new Physics(position.ToCPos() / new CPos(2,1,1), 0, Shape.NONE, 0, 0))
		{
			LayerPosition = position;
			var pos = position.ToCPos() / new CPos(2,1,1);
			Position = pos + new CPos(0,-512,0);
			Position += position.X % 2 != 0 ? CPos.Zero : new CPos(0,0,2048);
			Physics.Position += position.X % 2 != 0 ? new CPos(0, 512, 0) : new CPos(0, 1024, 0);
			renderPosition = pos + (position.X % 2 != 0 ? new CPos(-512, -1536, 0) : new CPos(-83, -512, 0));
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
			Renderable.CheckVisibility();
		}
	}
}
