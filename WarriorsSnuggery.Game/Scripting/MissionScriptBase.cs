using System;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Scripting
{
	public abstract class MissionScriptBase : ISaveable
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

		public virtual TextNodeSaver Save() => new TextNodeSaver();
		public virtual void Load(TextNode[] nodes) { }
	}
}
