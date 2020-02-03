namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Attach this to an actor to make it vulnerable and to have health.")]
	public class HealthPartInfo : PartInfo
	{
		[Desc("Maximal Health to archive.")]
		public readonly int MaxHealth;
		[Desc("Health when the actor is spawned.")]
		public readonly int StartHealth;

		public override ActorPart Create(Actor self)
		{
			return new HealthPart(self, this);
		}

		public HealthPartInfo(string internalName, MiniTextNode[] nodes) : base(internalName, nodes)
		{
			if (StartHealth == 0)
				StartHealth = MaxHealth;
		}
	}

	public class HealthPart : ActorPart
	{
		readonly HealthPartInfo info;

		public readonly int MaxHP;
		public readonly int StartHealth;
		public float HPRelativeToMax => health / (float)MaxHP;
		public int HP
		{
			get
			{
				return health;
			}
			set
			{
				health = value;
				if (health > MaxHP)
					health = MaxHP;
				if (health <= 0)
					health = 0;
			}
		}
		int health;

		public HealthPart(Actor self, HealthPartInfo info) : base(self)
		{
			this.info = info;

			MaxHP = info.MaxHealth;
			StartHealth = info.StartHealth;

			HP = StartHealth;
		}
	}
}
