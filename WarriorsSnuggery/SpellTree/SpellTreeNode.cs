namespace WarriorsSnuggery
{
	public class SpellTreeNode
	{
		public readonly string[] Before;
		public readonly MPos Position;
		public readonly int Cost;
		public readonly bool Unlocked;

		public readonly string InnerName;
		public readonly string Name;
		public readonly Spell Spell;

		public readonly string Icon;

		public SpellTreeNode(MiniTextNode[] nodes, string name)
		{
			InnerName = name;
			Name = name.Replace('_', ' ');
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Before":
						Before = node.Convert<string[]>();
						break;
					case "Cost":
						Cost = node.Convert<int>();
						break;
					case "Unlocked":
						Unlocked = node.Convert<bool>();
						break;
					case "Position":
						Position = node.Convert<MPos>();
						break;
					case "Spell":
						var effect = EffectType.NONE;
						var value = 0f;
						var manaCost = 0;
						var duration = 0;
						var cooldown = 0;

						effect = node.Convert<EffectType>();

						foreach (var child in node.Children)
						{
							switch (child.Key)
							{
								case "Value":
									value = child.Convert<float>();
									break;
								case "ManaCost":
									manaCost = child.Convert<int>();
									break;
								case "Duration":
									duration = child.Convert<int>();
									break;
								case "Cooldown":
									cooldown = child.Convert<int>();
									break;
							}
						}
						Spell = new Spell(effect, value, manaCost, duration, cooldown);
						break;
					case "Icon":
						Icon = node.Convert<string>();

						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "TechTreeNode");
				}
			}
		}
	}
}
