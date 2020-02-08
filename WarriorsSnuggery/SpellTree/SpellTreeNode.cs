using System;
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
		[Desc("Description of the spell. Shown in the spell tree window.")]
		public readonly string Description = "";

		public readonly string InnerName;
		public readonly string Name;

		[Desc("Spell effect.")]
		public readonly Spell Spell;

		[Desc("Icon of the spell.")]
		public readonly TextureInfo Icon;

		public readonly Texture[] Textures;

		public CPos VisualPosition
		{
			get { return new CPos(-6584, -2048, 0) + new CPos(Position.X * 1536, Position.Y * 1536, 0); }
		}

		public SpellTreeNode(MiniTextNode[] nodes, string name, bool documentation = false)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (!documentation)
			{
				InnerName = name;
				Name = name.Replace('_', ' ');
				Textures = SpriteManager.AddTexture(Icon);
			}
		}

		public string[] GetInformation(bool showDesc)
		{
			var res = new string[showDesc ? 3 : 2];
			res[0] = Color.Grey + "Mana use: " + new Color(0.5f, 0.5f, 1f) + Spell.ManaCost;
			res[1] = Color.Grey + "Reload: " + Color.Green + Math.Round(Spell.Cooldown / (float)Settings.UpdatesPerSecond, 2) + Color.Grey + " Seconds";
			if (showDesc)
				res[2] = Color.Grey + Description;

			return res;
		}
	}
}
