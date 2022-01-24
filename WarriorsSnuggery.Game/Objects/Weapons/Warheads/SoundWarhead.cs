using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons.Warheads
{
	public class SoundWarhead : IWarhead
	{
		[Require, Desc("Sound to play on impact.")]
		public readonly SoundType Sound;

		public SoundWarhead(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			var sound = new Sound(Sound);
			sound.Play(target.Position, false);
		}
	}
}
