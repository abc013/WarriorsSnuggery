using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons.Projectiles
{
	public class InstantHitProjectile : IProjectile
	{
		[Desc("Chance of the weapon to hit.")]
		public readonly float HitChance = 1.0f;
		[Desc("Determines whether only the target should be damaged when hit.")]
		public readonly bool Splash = true;

		public InstantHitProjectile(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}

		public BatchSequence GetTexture()
		{
			return null;
		}
	}
}
