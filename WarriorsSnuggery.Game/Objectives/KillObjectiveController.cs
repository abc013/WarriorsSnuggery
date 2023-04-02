using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objectives
{
	public class KillObjectiveController : ObjectiveController
	{
		public override string MissionString => "Wipe out all enemies on the map!";

		public KillObjectiveController(Game game) : base(game) { }

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
            var actors = Game.World.ActorLayer.NonNeutralActors;
			if (!actors.Any(a => a.Team != Actor.PlayerTeam && a.WorldPart != null && a.WorldPart.KillForVictory && !(a.Team == Actor.PlayerTeam || a.Team == Actor.NeutralTeam)))
				Game.VictoryConditionsMet();
		}
	}
}
