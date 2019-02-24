using System;

namespace WarriorsSnuggery.Objects
{
	public class CheckBoxType
	{
		public readonly GraphicsObject Default;
		public readonly GraphicsObject Checked;
		public readonly GraphicsObject Click;

		public readonly float Height;
		public readonly float Width;

		public readonly int Particles;
		public readonly ParticleType Particle;

		public CheckBoxType(GraphicsObject @default, GraphicsObject @checked, GraphicsObject click, float height, float width, int particles = 0, ParticleType particle = null)
		{
			Default = @default;
			Checked = @checked;
			Click = click;

			Height = height * 512;
			Width = width * 512;

			Particles = particles;
			Particle = particle;
		}
	}
}
