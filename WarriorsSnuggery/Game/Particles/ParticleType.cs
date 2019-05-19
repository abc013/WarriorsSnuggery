using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Objects
{
	public class ParticleType
	{
		public readonly TextureInfo Texture;

		[Desc("Force that is being applied.")]
		public readonly CPos Force;
		[Desc("Random Force.", "This is applied under Force with the name Random.")]
		public readonly CPos RanVelocity;

		[Desc("Rotation.")]
		public readonly int Rotation;

		[Desc("Scale.")]
		public readonly float Scale;
		[Desc("Random scale over time.", "This is applied unter Scale with the name Random.")]
		public readonly float RanScale;

		[Desc("Time in which the particle will exist.")]
		public readonly int Tick;
		[Desc("Time in which the particle will begin to dissolve.", "This time will be added on the time given in Tick.")]
		public readonly int DissolveTick;

		public ParticleType(TextureInfo texture, int tick, int dissolveTick, CPos force, int rotation, CPos ranVelocity, float scale, float ranScale)
		{
			Texture = texture;
			Tick = tick;
			DissolveTick = dissolveTick;
			Force = force;
			Rotation = rotation;
			RanVelocity = ranVelocity;
			Scale = scale;
			RanScale = ranScale;
		}
	}
}
