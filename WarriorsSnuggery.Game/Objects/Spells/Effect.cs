using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Spells
{
	public class Effect
	{
		[Desc("Decides when to activate the effect.")]
		public readonly EffectActivationType Activation = EffectActivationType.INSTANT;

		[Desc("Type of the Effect.")]
		public readonly EffectType Type;
		[Desc("Strength/Intensity of the effect.", "Value effect depends on the effect type.")]
		public readonly float Value = 1f;

		[Desc("Duration of the effect.")]
		public readonly int Duration;

		[Desc("Spawns particles while the effect is active.")]
		public readonly ParticleSpawner Particles;
		[Desc("Frequency of spawning Particles.")]
		public readonly int ParticleTick = 1;
		[Desc("Sound while the effect is active.")]
		public readonly SoundType Sound;

		public Effect(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}
	}
}
