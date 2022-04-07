using System;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Scripting
{
	public abstract class MissionScriptBase
	{
		public readonly PackageFile PackageFile;

		protected readonly Game game;
		protected readonly World world;

		public Action Tick { get; protected set; } = () => { };
		public Action OnStart { get; protected set; } = () => { };
		public Action OnFinish { get; protected set; } = () => { };
		public Action OnWin { get; protected set; } = () => { };
		public Action OnLose { get; protected set; } = () => { };

		public MissionScriptBase(PackageFile file, Game game)
		{
			PackageFile = file;
			this.game = game;
			world = game.World;
		}

		public virtual object[] GetState() => new object[0];

		public virtual void LoadState(TextNode[] nodes) { }
	}
}
