using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Attach this to an actor to make it vulnerable and to have health.")]
	public class HealthPartInfo : PartInfo
	{
		[Require, Desc("Maximal Health to archive.")]
		public readonly int MaxHealth;
		[Desc("Health when the actor is spawned.")]
		public readonly int StartHealth;

		[Desc("Determines whether damage dealt to this actor can be received by another.")]
		public readonly bool ImmuneToVampirism = true;

		public HealthPartInfo(PartInitSet set) : base(set)
		{
			if (StartHealth <= 0 || StartHealth > MaxHealth)
				StartHealth = MaxHealth;
		}
	}

	public class HealthPart : ActorPart, ITick, ISaveLoadable
	{
		readonly HealthPartInfo info;

		public int MaxHP => info.MaxHealth;
		public int StartHealth => info.StartHealth;
		public bool ImmuneToVampirism => info.ImmuneToVampirism;

		public float RelativeHP
		{
			get => health / (float)MaxHP;
			set => health = (int)(value * MaxHP);
		}

		public int HP
		{
			get => health;
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

		public HealthPart(Actor self, HealthPartInfo info) : base(self, info)
		{
			this.info = info;

			HP = StartHealth;
		}

		public void OnLoad(PartLoader loader)
		{
			foreach (var node in loader.GetNodes(typeof(HealthPart), Specification))
			{
				if (node.Key == "Health")
					HP = node.Convert<int>();
			}
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this);

			saver.Add("Health", HP, StartHealth);

			return saver;
		}

		public void Tick()
		{
			foreach (var effect in Self.GetActiveEffects(EffectType.HEALTH))
				HP += (int)effect.Effect.Value;

			if (Self.World.Game.LocalTick % 2 == 0 && Self.CurrentTerrain != null && Self.CurrentTerrain.Type.Damage != 0)
				HP -= Self.CurrentTerrain.Type.Damage;

			if (HP <= 0)
				Self.Killed(null);
		}
	}
}