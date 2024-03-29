﻿using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery.Objects.Weapons.Warheads
{
	public class ParticleWarhead : IWarhead
	{
		[Require, Desc("Particlespawner that will be used to spawn the particles.")]
		public readonly ParticleSpawner Spawner;

		public ParticleWarhead(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			world.Add(Spawner.Create(world, target.Position));
		}
	}
}
