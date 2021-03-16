using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Scripting
{
	public abstract class MissionScriptBase
	{
		public readonly string File;

		protected readonly Game game;
		protected readonly World world;

		public MissionScriptBase(string file, Game game)
		{
			File = file;
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

		public virtual object[] GetState()
		{
			return null;
		}

		public virtual void LoadState(TextNode[] nodes)
		{

		}

		public virtual void Tick()
		{

		}
	}
}
