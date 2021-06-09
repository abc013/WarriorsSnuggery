using System;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Allows an actor to hover above the ground.")]
	public class HoverPartInfo : PartInfo
	{
		[Desc("Height of the actor.")]
		public readonly int Height;

		[Desc("Hovering strength.", "This will create a hover-effect for flying actors.")]
		public readonly int Hover;
		[Desc("Hovering speed.", "If set lower, the hover frequency decreases.")]
		public readonly int HoverSpeed = 32;

		public HoverPartInfo(PartInitSet set) : base(set) { }

		public override HoverPart Create(Actor self)
		{
			return new HoverPart(self, this);
		}
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
				self.Height += (int)(MathF.Sin(hoverTick++ / (float)info.HoverSpeed) * info.Hover);

			if (self.Mobility != null && self.Mobility.CanFly)
			{
				if (self.Height > DefaultHeight + info.Hover * 64)
					self.AccelerateHeightSelf(false);
				else if (self.Height < DefaultHeight - info.Hover * 64)
					self.AccelerateHeightSelf(true);
			}
		}
	}
}
