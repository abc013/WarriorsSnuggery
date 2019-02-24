﻿/*
 * User: Andreas
 * Date: 08.12.2017
 * 
 */
using System;

namespace WarriorsSnuggery.Objects
{
	public class Particle : GameObject, ITick
	{
		int current;
		int dissolve;

		readonly ParticleType info;
		readonly float scale;

		CPos velocity;

		public Particle(CPos pos, ParticleType info) : base(pos, new SpriteRenderable(info.Texture.GetTextures(), tick: info.Texture.Tick))
		{
			this.info = info;
			current = info.Start;
			dissolve = info.DissolveTick;
			scale = (Program.SharedRandom.Next(2) - 1) * info.RanScale * info.Scale + info.Scale;
			Scale = scale;

			float ranVelocity() => ((float)Program.SharedRandom.NextDouble() * 2) - 1;

			velocity = new CPos((int) (ranVelocity() * info.RanVelocity.X), (int) (ranVelocity() * info.RanVelocity.Y), (int) (ranVelocity() * info.RanVelocity.Z));
		}

		public override void Tick()
		{
			base.Tick();

			Rotation += new CPos(0, 0, info.Rotation);
			Position += velocity;
			velocity += info.Force;

			if (!info.RanScale.Equals(0f))
				Scale = scale * (float) Math.Sin(current * info.RanScale);

			if (current-- <= 0)
			{
				if (dissolve-- <= 0)
				{
					Dispose();
					return;
				}
				else
				{
					Renderable.setColor(new Color(1f,1f,1f,((float) dissolve)/info.DissolveTick));
				}
			}
		}
	}
}
