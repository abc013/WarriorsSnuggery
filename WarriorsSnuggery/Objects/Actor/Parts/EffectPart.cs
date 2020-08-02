using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Parts
{
	public class EffectPart : ITick
	{
		public readonly Spell Spell;
		readonly Actor self;

		int tick;
		int particleTick;
		public bool Active = true;

		public EffectPart(Actor self, Spell spell)
		{
			Spell = spell;
			this.self = self;
			tick = spell.Duration;
			particleTick = spell.ParticleTick;

			if (Spell.Sound != null)
			{
				var sound = new Sound(Spell.Sound);
				sound.Play(self.Position, false);
			}
		}

		public EffectPart(Actor self, List<MiniTextNode> nodes, uint id)
		{
			this.self = self;
			var parent = nodes.FirstOrDefault(n => n.Key == "EffectPart" && n.Value == id.ToString());
			if (parent == null)
				return;

			foreach(var child in parent.Children)
			{
				switch (child.Key)
				{
					case "Spell":
						Spell = new Spell(child.Children.ToArray());

						break;
					case "Tick":
						tick = child.Convert<int>();

						break;
					case "ParticleTick":
						particleTick = child.Convert<int>();

						break;
					case "Active":
						Active = child.Convert<bool>();

						break;
				}
			}
		}

		public List<string> Save(uint id)
		{
			var list = new List<string>
			{
				id.ToString() + "=",
				"\tSpell=",
				"\t\tCooldown=" + Spell.Cooldown,
				"\t\tDuration=" + Spell.Duration,
				"\t\tParticleTick=" + Spell.ParticleTick,
				"\t\tType=" + Spell.Type,
				"\t\tValue=" + Spell.Value
			};

			//if (Spell.Particles != null)
			//{
			//	if (Spell.Particles as spaw)
			//}

			if (tick != 0)
				list.Add("\tTick=" + tick);
			if (particleTick != 0)
				list.Add("\tParticleTick=" + particleTick);
			if (!Active)
				list.Add("\tActive=" + Active);

			return list;
		}

		public void Tick()
		{
			if (self.World.Game.Editor)
				return;

			if (Active && tick-- < 0)
				Active = false;

			if (Spell.Particles != null && particleTick-- < 0)
			{
				particleTick = Spell.ParticleTick;
				self.World.Add(Spell.Particles.Create(self.World, self.Position, self.Height));
			}
		}
	}
}
