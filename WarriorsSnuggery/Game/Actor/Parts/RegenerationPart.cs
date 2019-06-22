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

		public RegenerationPartInfo(MiniTextNode[] nodes) : base(nodes)
		{

		}
	}
	
	public class RegenerationPart : ActorPart
	{
		readonly RegenerationPartInfo info;
		int tick;

		public RegenerationPart(Actor self, RegenerationPartInfo info) : base(self)
		{
			this.info = info;
		}

		public override void Tick()
		{
			if (tick-- <= 0)
			{
				if (self.Health == null)
					throw new YamlInvalidNodeException("RegenerationPart needs HealthPart to operate.");

				self.Health.HP += info.Amount;
				tick = info.Time;
			}
		}

		public override void OnDamage(Actor damager, int damage)
		{
			tick += info.TimeAfterHit;
		}
	}
}
