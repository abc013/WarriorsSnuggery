using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Trophies;

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
				{ "MissionKillEnemies", game.Mode == GameMode.KILL_ENEMIES },
				{ "MissionFindExit", game.Mode == GameMode.FIND_EXIT },
				{ "MissionWaves", game.Mode == GameMode.WAVES },
				{ "IsMenu", game.Type == GameType.MAINMENU || game.Type == GameType.MENU },
				{ "KeyFound", false }
			};
		}

		public void Tick()
		{
			items["Enemies"] = game.World.Actors.Any(a => a.Team != Actor.PlayerTeam && a.Team != Actor.NeutralTeam);
			items["KeyFound"] = game.World.KeyFound;
		}

		public bool CheckCondition(Condition condition, Actor actor)
		{
			if (items.ContainsKey(condition.Type))
				return condition.Negate != items[condition.Type];

			// Condition is a local type, which means it depends on the actor
			switch (condition.Type)
			{
				case "True":
					return !condition.Negate;
				case "False":
					return condition.Negate;
				case "IsFriendly":
					return condition.Negate != (actor.Team == Actor.PlayerTeam);
				case "IsNeutral":
					return condition.Negate != (actor.Team == Actor.NeutralTeam);
				case "IsPlayer":
					return condition.Negate != actor.IsPlayer;
				case "IsEnemy":
					return condition.Negate != (actor.Team != Actor.PlayerTeam && actor.Team != Actor.NeutralTeam);
				case "IsBot":
					return condition.Negate != actor.IsBot;
				case "IsIdling":
					return condition.Negate != (actor.CurrentAction == ActorAction.IDLING);
				case "IsMoving":
					return condition.Negate != (actor.CurrentAction == ActorAction.MOVING);
				case "IsAttacking":
					return condition.Negate != (actor.CurrentAction == ActorAction.ATTACKING);
				case "IsAlive":
					if (actor.Health == null)
						return !condition.Negate;
					return condition.Negate != (actor.Health.HPRelativeToMax != 10);
				case "IsDamaged":
					if (actor.Health == null)
						return condition.Negate;
					return condition.Negate != (actor.Health.HPRelativeToMax != 1);
				case "SlightDamage":
					if (actor.Health == null)
						return condition.Negate;
					return condition.Negate != (actor.Health.HP > actor.Health.MaxHP / 4f && actor.Health.HP < 3 * actor.Health.MaxHP / 4f);
				case "HeavyDamage":
					if (actor.Health == null)
						return condition.Negate;
					return condition.Negate != (actor.Health.HP <= actor.Health.MaxHP / 4f);
			}

			foreach (var pair in TrophyManager.Trophies)
			{
				var trophy = pair.Value;
				if (trophy.ConditionName != string.Empty && condition.Type == trophy.ConditionName)
					return condition.Negate != game.Statistics.UnlockedTrophies.Contains(pair.Key);
			}

			throw new Exception(string.Format("Invalid condition: {0}", condition.Type));
		}
	}
}
