using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors.Parts
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

		public RegenerationPartInfo(PartInitSet set) : base(set) { }
	}

	public class RegenerationPart : ActorPart, ITick, INoticeDamage, ISaveLoadable
	{
		readonly RegenerationPartInfo info;
		int tick;

		public RegenerationPart(Actor self, RegenerationPartInfo info) : base(self, info)
		{
			this.info = info;
		}

		public void OnLoad(PartLoader loader)
		{
			foreach (var node in loader.GetNodes(typeof(RegenerationPart), Specification))
			{
				if (node.Key == "Tick")
					tick = node.Convert<int>();
			}
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this, Specification);

			saver.Add("Tick", tick, 0);

			return saver;
		}

		public void Tick()
		{
			if (tick-- <= 0)
			{
				if (Self.Health == null)
					throw new InvalidNodeException("RegenerationPart needs HealthPart to operate.");

				Self.Health.HP += info.Amount;
				tick = info.Time;
			}
		}

		public void OnDamage(Actor damager, int damage)
		{
			tick = info.TimeAfterHit;
		}
	}
}
