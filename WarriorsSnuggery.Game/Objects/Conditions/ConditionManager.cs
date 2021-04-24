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
				{ "MissionKillEnemies", game.ObjectiveType == ObjectiveType.KILL_ENEMIES },
				{ "MissionFindExit", game.ObjectiveType == ObjectiveType.FIND_EXIT },
				{ "MissionWaves", game.ObjectiveType == ObjectiveType.SURVIVE_WAVES },
				{ "IsMenu", game.IsMenu },
				{ "KeyFound", game.Save.KeyFound }
			};
		}

		public void Tick()
		{
			items["Enemies"] = game.World.ActorLayer.NonNeutralActors.Any(a => a.Team != Actor.PlayerTeam);
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
					return condition.Negate != (actor.CurrentAction.Type == ActionType.IDLE);
				case "IsMoving":
					return condition.Negate != (actor.CurrentAction.Type == ActionType.MOVE);
				case "StartsAttacking":
					return condition.Negate != (actor.CurrentAction.Type == ActionType.PREPARE_ATTACK);
				case "IsAttacking":
					return condition.Negate != (actor.CurrentAction.Type == ActionType.ATTACK);
				case "EndsAttacking":
					return condition.Negate != (actor.CurrentAction.Type == ActionType.END_ATTACK);
				case "IsAlive":
					if (actor.Health == null)
						return !condition.Negate;
					return condition.Negate != (actor.Health.RelativeHP != 10);
				case "IsDamaged":
					if (actor.Health == null)
						return condition.Negate;
					return condition.Negate != (actor.Health.RelativeHP != 1);
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
					return condition.Negate != game.Save.UnlockedTrophies.Contains(pair.Key);
			}

			throw new Exception(string.Format("Invalid condition: {0}", condition.Type));
		}
	}
}
