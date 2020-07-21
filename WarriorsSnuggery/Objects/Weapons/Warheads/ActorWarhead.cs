namespace WarriorsSnuggery.Objects.Weapons
{
	public class ActorWarhead : IWarhead
	{
		[Desc("Actortype that will be spawned.")]
		public readonly string Type;

		[Desc("Actor is bot.")]
		public readonly bool IsBot;
		[Desc("Actor uses the team of its origin.")]
		public readonly bool UseTeam = true;

		public ActorWarhead(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			world.Add(ActorCreator.Create(world, Type, target.Position, weapon.Team, IsBot));
		}
	}
}
