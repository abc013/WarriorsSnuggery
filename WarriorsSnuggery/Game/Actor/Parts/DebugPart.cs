using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects.Parts
{
	class DebugPart : ActorPart
	{
		readonly ColoredRectRenderable center;
		readonly ColoredCircleRenderable weaponRange;

		public DebugPart(Actor self) : base(self)
		{
			center = new ColoredRectRenderable(Color.Blue, 0.1f, DrawMethod.TRIANGLE);
			var color = self.IsPlayer ? Color.Red : new Color(127, 0, 0);
			if (self.ActiveWeapon != null)
				weaponRange = new ColoredCircleRenderable(color, self.ActiveWeapon.Type.MaxRange / 1024f * 2, 16, DrawMethod.LINELOOP);
			else
				weaponRange = new ColoredCircleRenderable(color, 0f, 2, DrawMethod.LINELOOP);
			WorldRenderer.RenderAfter(center);
			WorldRenderer.RenderAfter(weaponRange);
		}

		public override void Tick()
		{
			center.SetPosition(self.Position);
			weaponRange.SetPosition(self.Position);
		}

		public override void OnDispose()
		{
			WorldRenderer.RemoveRenderAfter(center);
			WorldRenderer.RemoveRenderAfter(weaponRange);
			center.Dispose();
			weaponRange.Dispose();
		}
	}
}
