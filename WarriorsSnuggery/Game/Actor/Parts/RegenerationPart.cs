using System;

namespace WarriorsSnuggery.Objects.Parts
{
	public class RegenerationPartInfo : PartInfo
	{
		public readonly int Amount;
		public readonly int Time;
		public readonly int TimeAfterHit;

		public override ActorPart Create(Actor self)
		{
			return new RegenerationPart(self, this);
		}

		public RegenerationPartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Amount":
						Amount = node.ToInt();
						break;
					case "Time":
						Time = node.ToInt();
						break;
					case "TimeAfterHit":
						TimeAfterHit = node.ToInt();
						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "RegenerationPart");
				}
			}
		}
	}

	/// <summary>
	/// Activates regeneration features.
	/// Crashes if theres no Health.
	/// </summary>
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
