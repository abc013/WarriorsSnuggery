using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors
{
	public enum ActionType
	{
		IDLE = 0,
		PREPARE_MOVE = 1,
		MOVE = 2,
		END_MOVE = 4,
		PREPARE_ATTACK = 8,
		ATTACK = 16,
		END_ATTACK = 32
	}

	public class ActorAction
	{
		public readonly ActorAction Following;

		public readonly ActionType Type;

		public int CurrentTick { get; private set; }

		public bool ActionOver => CurrentTick < 0;

		public ActorAction(ActionType type, int duration, ActorAction following = null)
		{
			Type = type;
			CurrentTick = duration;

			Following = following;
		}

		public ActorAction(List<TextNode> nodes)
		{
			foreach (var child in nodes)
			{
				switch (child.Key)
				{
					case "Following":
						Following = new ActorAction(child.Children);

						break;
					case "Type":
						Type = child.Convert<ActionType>();

						break;
					case "CurrentTick":
						CurrentTick = child.Convert<int>();

						break;
				}
			}
		}

		public void ExtendAction(int ticks)
		{
			CurrentTick += ticks;
		}

		public bool IsOverOrCanceled(ActionType actions, bool attackWhileMove)
		{
			CurrentTick--;

			if (actions == ActionType.IDLE || actions == Type)
				return ActionOver;

			if (Type == ActionType.ATTACK || Type == ActionType.END_ATTACK)
				return ActionOver;

			if (!attackWhileMove && doesAction(actions, ActionType.ATTACK | ActionType.END_ATTACK))
				return true;

			if (doesAction(actions, ActionType.MOVE | ActionType.PREPARE_MOVE) && Type == ActionType.PREPARE_ATTACK)
				return true;

			return ActionOver;
		}

		public List<string> Save()
		{
			var list = new List<string>();
			list.Add($"{nameof(ActorAction)}=");

			if (Following != null)
			{
				var appends = Following.Save();
				foreach (var append in appends)
					list.Add("\t" + append);
			}

			list.Add($"\tType={Type}");
			list.Add($"\tCurrentTick={CurrentTick}");
			return list;
		}

		bool doesAction(ActionType actions, ActionType type)
		{
			return (actions & type) != 0;
		}
	}
}
