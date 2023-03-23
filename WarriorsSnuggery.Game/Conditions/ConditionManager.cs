using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Trophies;

namespace WarriorsSnuggery.Conditions
{
	public class ConditionManager : ISaveable
	{
		readonly Game game;
		readonly Dictionary<string, bool> items;
		readonly Dictionary<string, bool> customItems = new Dictionary<string, bool>();

		public ConditionManager(Game game)
		{
			this.game = game;
			items = new Dictionary<string, bool>()
			{
				{ "Enemies", true },
				{ "MissionKillEnemies", game.ObjectiveType == ObjectiveType.KILL_ENEMIES },
				{ "MissionFindExit", game.ObjectiveType == ObjectiveType.FIND_EXIT },
				{ "MissionWaves", game.ObjectiveType == ObjectiveType.SURVIVE_WAVES },
				{ "IsMenu", game.MissionType.IsMenu() },
				{ "KeyFound", game.Stats.KeyFound }
			};
		}

		public void Tick()
		{
			items["Enemies"] = game.World.ActorLayer.NonNeutralActors.Any(a => a.Team != Actor.PlayerTeam);
			items["KeyFound"] = game.Stats.KeyFound;
		}

		public bool CheckCondition(Condition condition, Actor actor)
		{
			if (items.ContainsKey(condition.Type))
				return condition.Negate != items[condition.Type];

			if (customItems.ContainsKey(condition.Type))
				return condition.Negate != customItems[condition.Type];

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
					return condition.Negate != actor.DoesAction(ActionType.IDLE);
				case "StartsMoving":
					return condition.Negate != actor.DoesAction(ActionType.PREPARE_MOVE);
				case "IsMoving":
					return condition.Negate != actor.DoesAction(ActionType.MOVE);
				case "EndsMoving":
					return condition.Negate != actor.DoesAction(ActionType.END_MOVE);
				case "StartsAttacking":
					return condition.Negate != actor.DoesAction(ActionType.PREPARE_ATTACK);
				case "IsAttacking":
					return condition.Negate != actor.DoesAction(ActionType.ATTACK);
				case "EndsAttacking":
					return condition.Negate != actor.DoesAction(ActionType.END_ATTACK);
				case "IsSpawning":
					return condition.Negate != actor.DoesAction(ActionType.SPAWN);
				case "IsDespawning":
					return condition.Negate != actor.DoesAction(ActionType.DESPAWN);
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

			foreach (var key in TrophyCache.Trophies.Keys)
			{
				var trophy = TrophyCache.Trophies[key];
				if (!string.IsNullOrEmpty(trophy.ConditionName) && condition.Type == trophy.ConditionName)
					return condition.Negate != game.Stats.TrophyUnlocked(key);
			}

			throw new Exception($"Invalid condition: {condition.Type}");
		}

		public void SetCondition(string name, bool value)
		{
			if (!customItems.ContainsKey(name))
			{
				customItems.Add(name, value);
				return;
			}

			customItems[name] = value;
		}

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			foreach (var condition in customItems)
				saver.Add(condition.Key, condition.Value);

			return saver;
		}
	}
}
