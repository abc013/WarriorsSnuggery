using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public class PhysicsObject : PositionableObject
	{
		public readonly SimplePhysics Physics;

		[Save]
		public override int Height
		{
			get => base.Height;
			set
			{
				base.Height = value;

				Physics.Height = value;
			}
		}

		[Save]
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				Physics.Position = value;
			}
		}

		public PhysicsObject(CPos pos, BatchRenderable renderable, SimplePhysics physics = null) : base(pos, renderable)
		{
			Physics = physics ?? SimplePhysics.Empty;
		}

		public override void Dispose()
		{
			foreach (var sector in Physics.Sectors)
				sector.Leave(this);

			base.Dispose();
		}
	}
}
