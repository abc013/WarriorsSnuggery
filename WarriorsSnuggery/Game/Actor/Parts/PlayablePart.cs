using System;

namespace WarriorsSnuggery.Objects.Parts
{
	public class PlayablePartInfo : PartInfo
	{
		public readonly bool Playable;
		public readonly bool Unlocked;

		public readonly int UnlockCost;
		public readonly int ChangeCost;

		public override ActorPart Create(Actor self)
		{
			return new PlayablePart(self, this);
		}

		public PlayablePartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Playable":
						Playable = node.ToBoolean();

						break;
					case "Unlocked":
						Unlocked = node.ToBoolean();

						break;
					case "UnlockCost":
						UnlockCost = node.ToInt();

						break;
					case "Cost":
						ChangeCost = node.ToInt();

						break;
					default:
						throw new YamlUnknownNodeException(node.Key);
				}
			}
		}
	}

	public class PlayablePart : ActorPart
	{
		readonly PlayablePartInfo info;

		public bool Playable
		{
			get { return info.Playable; }
			set { }
		}

		public bool Unlocked
		{
			get { return info.Unlocked; }
			set { }
		}

		public int UnlockCost
		{
			get { return info.UnlockCost; }
			set { }
		}

		public int ChangeCost
		{
			get { return info.ChangeCost; }
			set { }
		}

		public PlayablePart(Actor self, PlayablePartInfo info) : base(self)
		{
			this.info = info;
		}
	}
}
