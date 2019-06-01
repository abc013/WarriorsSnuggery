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

		public readonly string Icon;

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
						var effect = EffectType.NONE;
						var value = 0f;
						var duration = 0;
						var occurence = EffectOccurTypes.FIRST;
						var cooldown = 0;

						effect = (EffectType) node.ToEnum(typeof(EffectType));

						foreach (var child in node.Children)
						{
							switch(child.Key)
							{
								case "Value":
									value = child.ToFloat();
									break;
								case "Duration":
									duration = child.ToInt();
									break;
								case "Occurence":
									occurence = (EffectOccurTypes)child.ToEnum(typeof(EffectOccurTypes));
									break;
								case "Cooldown":
									cooldown = child.ToInt();
									break;
							}
						}
						Effect = new Effect(effect, value, duration, occurence, cooldown);
						break;
					case "Icon":
						Icon = node.Value;

						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "TechTreeNode");
				}
			}
		}
	}
}
