namespace WarriorsSnuggery.Objects.Weapons
{
	public class SoundWarhead : IWarhead
	{
		[Desc("Sound to play on impact.")]
		public readonly SoundType Sound;

		public SoundWarhead(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			var sound = new Sound(Sound);
			sound.Play(target.Position, false);
		}
	}
}
