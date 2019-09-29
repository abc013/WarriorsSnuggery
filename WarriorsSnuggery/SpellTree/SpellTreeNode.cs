using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Spells
{
	public class SpellTreeNode
	{
		[Desc("Spells that have to be unlocked before this one can be unlocked.")]
		public readonly string[] Before;
		[Desc("Graphical position in the spelltree.")]
		public readonly MPos Position;
		[Desc("Determines the amount of money which has to be spent in order to buy this spell.")]
		public readonly int Cost;
		[Desc("If true, the spell is unlocked from the beginning.")]
		public readonly bool Unlocked;

		public readonly string InnerName;
		public readonly string Name;

		[Desc("Spell effect.")]
		public readonly Spell Spell;

		[Desc("Icon of the spell.")]
		public readonly TextureInfo Icon;

		public readonly IImage[] Images;

		public SpellTreeNode(MiniTextNode[] nodes, string name)
		{
			Loader.PartLoader.SetValues(this, nodes);

			InnerName = name;
			Name = name.Replace('_', ' ');
			Images = SpriteManager.AddTexture(Icon);
		}
	}
}
