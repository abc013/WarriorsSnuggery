using System;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objectives
{
	public abstract class ObjectiveController : ITick, ILoadable, ISaveable
	{
		protected readonly Game Game;
		protected readonly Random Random;

		public abstract string MissionString { get; }

		public ObjectiveController(Game game)
		{
			Game = game;
			Random = new Random(game.Seed);
		}
		
		public abstract void Load(TextNodeInitializer initializer);

		public abstract void Tick();

		public abstract TextNodeSaver Save();

		public static ObjectiveController GetController(Game game, ObjectiveType type)
		{
			switch(type)
			{
				case ObjectiveType.SURVIVE_WAVES:
					return new WaveObjectiveController(game);
				case ObjectiveType.KILL_ENEMIES:
					return new KillObjectiveController(game);
				case ObjectiveType.FIND_EXIT:
					return new FindObjectiveController(game);
				default:
					return null;
			}
		}
	}
}
