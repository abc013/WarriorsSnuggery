using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors;

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
			if (StartHealth <= 0 || StartHealth > MaxHealth)
				StartHealth = MaxHealth;
		}
	}

	public class HealthPart : ActorPart
	{
		readonly HealthPartInfo info;

		public int MaxHP => info.MaxHealth;
		public int StartHealth => info.StartHealth;

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

		public HealthPart(Actor self, HealthPartInfo info) : base(self)
		{
			this.info = info;

			HP = StartHealth;
		}

		public override void OnLoad(List<MiniTextNode> nodes)
		{
			var parent = nodes.FirstOrDefault(n => n.Key == "HealthPart" && n.Value == info.InternalName);
			if (parent == null)
				return;

			foreach (var node in parent.Children)
			{
				if (node.Key == "Health")
					HP = node.Convert<int>();
			}
		}

		public override PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			saver.Add("Health", HP, StartHealth);

			return saver;
		}

		public override void Tick()
		{
			if (self.World.Game.Editor)
				return;

			if (self.World.Game.LocalTick % 2 == 0 && self.CurrentTerrain != null && self.CurrentTerrain.Type.Damage != 0)
				HP -= self.CurrentTerrain.Type.Damage;
		}
	}
}