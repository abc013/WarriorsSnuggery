﻿using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Adds a weapon to the object.", "IMPORTANT NOTE: Currently, shroud is only supported for teams 0-9. If you use higher team values, the game will crash!")]
	public class RevealsShroudPartInfo : PartInfo
	{
		[Desc("Range of revealing shroud.", "Given in half terrain dimension. (2 = 1 terrain size)")]
		public readonly int Range = 0;

		[Desc("Interval in which the game should check for revealled shroud by this actor.")]
		public readonly int Interval = 0;

		public RevealsShroudPartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new RevealsShroudPart(self, this);
		}
	}

	public class RevealsShroudPart : ActorPart, ITick, INoticeMove
	{
		readonly RevealsShroudPartInfo info;
		int tick;
		bool firstActive;

		public int Range => info.Range;

		public RevealsShroudPart(Actor self, RevealsShroudPartInfo info) : base(self)
		{
			this.info = info;
			firstActive = true;
		}

		public override void OnLoad(List<TextNode> nodes)
		{
			var parent = nodes.FirstOrDefault(n => n.Key == "RevealsShroudPart" && n.Value == info.InternalName);
			if (parent == null)
				return;

			foreach (var node in parent.Children)
			{
				if (node.Key == "Tick")
					tick = node.Convert<int>();
			}
		}

		public override PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			saver.Add("Tick", tick, 0);

			return saver;
		}

		public void OnMove(CPos old, CPos speed)
		{
			if (tick < 0)
			{
				self.World.ShroudLayer.RevealShroudCircular(self.World, self.Team, self.Position, self.Height, info.Range);
				tick = info.Interval;
			}
		}

		public void Tick()
		{
			tick--;
			if (firstActive)
			{
				self.World.ShroudLayer.RevealShroudCircular(self.World, self.Team, self.Position, self.Height, info.Range, true);
				firstActive = false;
			}
		}
	}
}
