using System;

namespace WarriorsSnuggery.Objects.Parts
{
	public class HealthPartInfo : PartInfo
	{
		public readonly int MaxHealth;
		public readonly int StartHealth;

		public override ActorPart Create(Actor self)
		{
			return new HealthPart(self, this);
		}

		public HealthPartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "MaxHP":
					case "MaxHealth":
						MaxHealth = node.ToInt();
						break;
					case "HP":
					case "StartHealth":
						StartHealth = node.ToInt();
						break;
					default:
						throw new YamlUnknownNodeException(node.Key);
				}
			}
			if (StartHealth == 0)
				StartHealth = MaxHealth;
		}
	}

	/// <summary>
	/// Activates health features.
	/// </summary>
	public class HealthPart : ActorPart
	{
		readonly HealthPartInfo info;

		public readonly int MaxHP;
		public readonly int StartHealth;
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
