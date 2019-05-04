namespace WarriorsSnuggery.Objects.Parts
{
	/// <summary>
	/// Shows shape of physics.
	/// </summary>
	class DebugPart : ActorPart
	{
		//readonly GameObject renderable;
		readonly PhysicsObject center;
		readonly PhysicsObject weaponRange;

		public DebugPart(Actor self) : base(self)
		{
			center = new ColoredRect(self.Position, Color.Blue, 0.1f);
			var color = self.IsPlayer ? Color.Red : new Color(127, 0, 0);
			if (self.ActiveWeapon != null)
				weaponRange = new ColoredCircle(self.Position, color, self.ActiveWeapon.Type.MaxRange / 1024f * 2, isFilled: false);
			else
				weaponRange = new ColoredCircle(self.Position, color, 0f, 2);
			WorldRenderer.RenderAfter(center);
			WorldRenderer.RenderAfter(weaponRange);
			//if (self.Physics == null)
			//	return;
			//var radius = self.Physics.Radius;

			//switch (self.Physics.Shape)
			//{
			//	case Shape.CIRCLE:
			//		renderable = new ColoredCircle(self.Position, Color.Cyan, radius * 2 / 1024f, 16, isFilled: false);
			//		break;
			//	case Shape.RECTANGLE:
			//		renderable = new ColoredRect(self.Position, Color.Cyan, radius * 2 / 1024f, isFilled: false);
			//		break;
			//	case Shape.LINE_HORIZONTAL:
			//		var debugLine = new ColoredLine(self.Position - new CPos(0, radius, -10240), Color.Cyan, radius * 2 / 1024f)
			//		{
			//			Rotation = new CPos(0, 0, 90)
			//		};
			//		renderable = debugLine;
			//		break;
			//	case Shape.LINE_VERTICAL:
			//		renderable = new ColoredLine(self.Position - new CPos(radius, 0, 0), Color.Cyan, radius * 2 / 1024f);
			//		break;
			//	default:
			//		return;
			//}
			//WorldRenderer.RenderAfter(renderable);
		}

		public override void Tick()
		{
			center.Position = self.Position;
			weaponRange.Position = self.Position;
		}

		public override void OnDispose()
		{
			WorldRenderer.RemoveRenderAfter(center);
			WorldRenderer.RemoveRenderAfter(weaponRange);
			center.Dispose();
			weaponRange.Dispose();
			//if (renderable != null)
			//	WorldRenderer.RemoveRenderAfter(renderable);
		}
	}
}
