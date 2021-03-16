using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Activates health regeneration features.", "The health rule is required for this.")]
	public class RegenerationPartInfo : PartInfo
	{
		[Desc("Amount of HP the player gets.")]
		public readonly int Amount;
		[Desc("Time between the regeneration steps.")]
		public readonly int Time;
		[Desc("Time between the regeneration step after a hit.")]
		public readonly int TimeAfterHit;

		public override ActorPart Create(Actor self)
		{
			return new RegenerationPart(self, this);
		}

		public RegenerationPartInfo(string internalName, List<MiniTextNode> nodes) : base(internalName, nodes)
		{

		}
	}

	public class RegenerationPart : ActorPart, ITick, INoticeDamage
	{
		readonly RegenerationPartInfo info;
		int tick;

		public RegenerationPart(Actor self, RegenerationPartInfo info) : base(self)
		{
			this.info = info;
		}

		public override void OnLoad(List<MiniTextNode> nodes)
		{
			var parent = nodes.FirstOrDefault(n => n.Key == "RegenerationPart" && n.Value == info.InternalName);
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

		public void Tick()
		{
			if (tick-- <= 0)
			{
				if (self.Health == null)
					throw new InvalidNodeException("RegenerationPart needs HealthPart to operate.");

				self.Health.HP += info.Amount;
				tick = info.Time;
			}
		}

		public void OnDamage(Actor damager, int damage)
		{
			tick = info.TimeAfterHit;
		}
	}
}
