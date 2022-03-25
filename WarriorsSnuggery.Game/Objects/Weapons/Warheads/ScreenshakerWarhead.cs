using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons.Warheads
{
	public class ScreenshakerWarhead : IWarhead
	{
		[Desc("Spell to be casted on the actors.")]
		public readonly int Strength;

		[Desc("Spell to be casted on the actors.")]
		public readonly int RandomStrength;

		[Desc("Spell to be casted on the actors.")]
		public readonly bool IgnoreDistance;

		public ScreenshakerWarhead(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			var strength = Strength + world.Game.SharedRandom.Next(RandomStrength);

			if (IgnoreDistance)
			{
				Screenshaker.ShakeStrength += strength;
				return;
			}

			var dist = (Camera.LookAt - target.Position).FlatDist;
			var multiplier = 1 - (dist / (Camera.CurrentZoom * Constants.TileSize));

			if (multiplier <= 0)
				return;

			Screenshaker.ShakeStrength += (int)(strength * multiplier);
		}
	}
}
