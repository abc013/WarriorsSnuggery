using System;
using System.Linq;
using System.Collections.Generic;

namespace WarriorsSnuggery.Objects.Conditions
{
	public class ConditionManager
	{
		readonly Game game;
		readonly Dictionary<string, bool> items;

		public ConditionManager(Game game)
		{
			this.game = game;
			items = new Dictionary<string, bool>()
			{
				{ "Enemies", true },
				{ "IsMenu", false },
				{ "MissionKillEnemies", game.Mode == GameMode.KILL_ENEMIES },
				{ "MissionFindExit", game.Mode == GameMode.FIND_EXIT },
				{ "MissionWaves", game.Mode == GameMode.WAVES }
			};
		}

		public void Tick()
		{
			items["Enemies"] = game.World.Actors.Any(a => a.Team != Actor.PlayerTeam && a.Team != Actor.NeutralTeam);
			items["IsMenu"] = game.Type == GameType.MAINMENU || game.Type == GameType.MENU;
		}

		public bool CheckCondition(Condition condition, Actor actor)
		{
			if (items.ContainsKey(condition.Type))
				return condition.Negate != items[condition.Type];

			// Condition is a local type, which means it depends on the actor
			switch(condition.Type)
			{
				case "IsPlayer":
					return actor.IsPlayer;
				case "IsEnemy":
					return actor.Team != Actor.PlayerTeam && actor.Team != Actor.NeutralTeam;
				case "IsBot":
					return actor.IsBot;
				case "IsDamaged":
					if (actor.Health == null)
						return false;
					return actor.Health.HPRelativeToMax != 1;
			}

			throw new Exception(string.Format("Invalid condition: {0}", condition.Type));
		}
	}
}
