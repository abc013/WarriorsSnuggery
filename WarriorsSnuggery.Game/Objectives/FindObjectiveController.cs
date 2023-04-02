using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objectives
{
	public class FindObjectiveController : ObjectiveController
	{
		public override string MissionString => "Search for the exit and gain access to it!";

		public FindObjectiveController(Game game) : base(game) { }

		public override void Load(TextNodeInitializer initializer)
		{
			initializer.SetSaveFields(this);
		}

		public override TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			saver.AddSaveFields(this);

			return saver;
		}

		public override void Tick()
		{
            // Victory condition is met automatically
		}
	}
}
