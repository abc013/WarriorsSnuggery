using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Spells
{
	public class Effect
	{
		[Desc("Type of the Spell.")]
		public readonly EffectType Type;
		[Desc("Strength/Intensity of the spell.", "Value effect depends on the effect type.")]
		public readonly float Value = 1f;

		[Desc("Duration of the effect.")]
		public readonly int Duration;

		[Desc("Spawns particles while the effect is active.")]
		public readonly ParticleSpawner Particles;
		[Desc("Frequency of spawning Particles.")]
		public readonly int ParticleTick;
		[Desc("Sound while the effect is active.")]
		public readonly SoundType Sound; //TODO

		public Effect(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}
	}
}
