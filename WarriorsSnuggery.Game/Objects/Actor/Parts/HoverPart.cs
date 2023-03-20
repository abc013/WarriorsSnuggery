using System;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Allows an actor to hover above the ground.")]
	public class HoverPartInfo : PartInfo
	{
		[Require, Desc("Default height of the actor.")]
		public readonly int Height;

		[Desc("Hovering strength.", "This will create a hover-effect for flying actors.")]
		public readonly int Hover;
		[Desc("Hovering speed.", "If set lower, the hover frequency decreases.")]
		public readonly int HoverSpeed = 32;

		public HoverPartInfo(PartInitSet set) : base(set) { }
	}

	public class HoverPart : ActorPart, ITick
	{
		readonly HoverPartInfo info;

		public int DefaultHeight => info.Height;

		int hoverTick;

		public HoverPart(Actor self, HoverPartInfo info) : base(self)
		{
			this.info = info;
		}

		public void Tick()
		{
			if (info.Hover > 0)
				self.Position += new CPos(0, 0, (int)(MathF.Sin(hoverTick++ / (float)info.HoverSpeed) * info.Hover));

			if (self.Mobile != null && self.Mobile.CanFly)
			{
				if (self.Position.Z > DefaultHeight + info.Hover * 64)
					self.AccelerateHeightSelf(false);
				else if (self.Position.Z < DefaultHeight - info.Hover * 64)
					self.AccelerateHeightSelf(true);
			}
		}
	}
}
