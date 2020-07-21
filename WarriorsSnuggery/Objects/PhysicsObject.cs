using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public class PhysicsObject : PositionableObject
	{
		public PhysicsSector[] PhysicsSectors = new PhysicsSector[0];
		public readonly SimplePhysics Physics;

		public override int Height
		{
			get { return base.Height; }
			set
			{
				base.Height = value;

				if (Physics != null)
					Physics.Height = value;
			}
		}

		public override CPos Position
		{
			get { return base.Position; }
			set
			{
				base.Position = value;

				if (Physics != null)
					Physics.Position = value;
			}
		}

		public PhysicsObject(CPos pos) : base(pos, null)
		{
			Physics = SimplePhysics.Empty;
		}

		public PhysicsObject(CPos pos, BatchRenderable renderable, SimplePhysics physics = null) : base(pos, renderable)
		{
			Physics = physics ?? SimplePhysics.Empty;
		}

		public override void Dispose()
		{
			foreach (var sector in PhysicsSectors)
				sector.Leave(this);

			base.Dispose();
		}
	}
}
