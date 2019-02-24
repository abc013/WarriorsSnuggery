using System;

namespace WarriorsSnuggery.Objects
{
	public class ParticleType
	{
		public readonly TextureInfo Texture;

		public readonly CPos Force;

		public readonly int Rotation;
		public readonly CPos RanVelocity;

		public readonly float Scale;
		public readonly float RanScale;
		
		public readonly int Start;
		public readonly int DissolveTick;

		public ParticleType(TextureInfo texture, int tick, int dissolveTick, CPos force, int rotation, CPos ranVelocity, float scale, float ranScale)
		{
			Texture = texture;
			Start = tick;
			DissolveTick = dissolveTick;
			Force = force;
			Rotation = rotation;
			RanVelocity = ranVelocity;
			Scale = scale;
			RanScale = ranScale;
		}
	}
}
