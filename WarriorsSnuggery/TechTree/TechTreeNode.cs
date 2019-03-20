using System;

namespace WarriorsSnuggery
{
	public class ITechTreeNode
	{
		public readonly string[] Before;
		public readonly MPos Position;
		public readonly int Cost;
		public readonly bool Unlocked;

		public readonly string InnerName;
		public readonly string Name;
		public readonly Effect Effect;

		public ITechTreeNode(MiniTextNode[] nodes, string name)
		{
			InnerName = name;
			Name = name.Replace('_',' ');
			foreach(var node in nodes)
			{
				switch(node.Key)
				{
					case "Before":
						Before = node.ToArray();
						break;
					case "Cost":
						Cost = node.ToInt();
						break;
					case "Unlocked":
						Unlocked = node.ToBoolean();
						break;
					case "Position":
						Position = node.ToMPos();
						break;
					case "Effect":
						break;
					default:
						throw new YamlUnknownNodeException(node.Key);
				}
			}
		}
	}

	public class TechTreeNode
	{
		public readonly ITechTreeNode Info;
		public bool Unlocked;

		public TechTreeNode(ITechTreeNode info, bool unlocked)
		{
			Info = info;
			Unlocked = unlocked || info.Unlocked;
		}
	}
}
