namespace WarriorsSnuggery.Scripting
{
	public abstract class MissionScriptBase
	{
		protected readonly Game game;
		protected readonly World world;

		public MissionScriptBase(Game game)
		{
			this.game = game;
			world = game.World;
		}

		public virtual void OnStart()
		{

		}

		public virtual void OnWin()
		{

		}

		public virtual void OnLose()
		{

		}

		public virtual void Tick()
		{

		}
	}
}
