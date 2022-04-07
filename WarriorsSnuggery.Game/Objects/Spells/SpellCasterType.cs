using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Spells
{
	public class SpellCasterType
	{
		[Desc("Spells that have to be unlocked before this one can be unlocked.")]
		public readonly string[] Before;
		[Require, Desc("Graphical position in the spelltree.")]
		public readonly MPos Position;

		[Desc("Determines the amount of money which has to be spent in order to buy this spell.")]
		public readonly int Cost;
		[Desc("Determines how much mana is used up by casting this spell.")]
		public readonly int ManaCost;

		[Desc("If true, the spell is unlocked from the beginning.")]
		public readonly bool Unlocked;
		[Desc("Description of the spell. Shown in the spell tree window.")]
		public readonly string Description = "";

		[Desc("Cooldown of the spell.")]
		public readonly int Cooldown;
		public readonly int Duration;

		public readonly string InnerName;
		public readonly string Name;

		[Require, Desc("Spell effect.")]
		public readonly Spell Spell;

		[Require, Desc("Icon of the spell.")]
		public readonly TextureInfo Icon;

		public CPos VisualPosition => new CPos(-6584, -2048, 0) + new CPos(Position.X * 1024, Position.Y * 1024, 0);

		public SpellCasterType(List<TextNode> nodes, string name)
		{
			TypeLoader.SetValues(this, nodes);

			if (Spell.Effects.Length > 0)
				Duration = Spell.Effects.Max(e => e.Duration);

			InnerName = name;
			Name = name.Replace('_', ' ');
		}

		public string[] GetDescription()
		{
			var effectCount = Spell.Effects.Length;
			var descCount = string.IsNullOrWhiteSpace(Description) ? 0 : 2;

			var array = new string[3 + effectCount + descCount];

			array[0] = Color.Grey + "Mana use: " + new Color(0.5f, 0.5f, 1f) + ManaCost;
			array[1] = Color.Grey + "Reload: " + Color.Green + Math.Round(Cooldown / (float)Settings.UpdatesPerSecond, 2) + Color.Grey + " seconds";
			array[2] = Color.White + $"This spell has {effectCount} effect{(effectCount > 1 ? "s" : "")}: ";
			for (int i = 0; i < effectCount; i++)
			{
				var effect = Spell.Effects[i];

				var name = effect.Type.ToString().ToLower();

				if (effect.Type == EffectType.DAMAGERANGE)
					name = "splash radius";

				var text = Color.Grey + "- ";
				text += effect.Type != EffectType.NONE ? Color.Yellow + name + Color.Grey : Color.Grey + "Useless effect";

				var direction = effect.Value > 1 ? "increased" : "decreased";
				switch (effect.Type)
				{
					case EffectType.HEALTH:
					case EffectType.MANA:
						text += $" {direction} by " + Color.Magenta + (int)(effect.Value * Settings.UpdatesPerSecond) + Color.Grey + " during " + Color.Cyan + Math.Round(effect.Duration / (float)Settings.UpdatesPerSecond, 2) + Color.Grey + " seconds";
						break;
					case EffectType.SHIELD:
						text += $" protecting from " + Color.Magenta + (int)effect.Value + Color.Grey + " damage for " + Color.Cyan + Math.Round(effect.Duration / (float)Settings.UpdatesPerSecond, 2) + Color.Grey + " seconds";
						break;
					case EffectType.STUN:
					case EffectType.INVISIBILITY:
						text += " for " + Color.Cyan + Math.Round(effect.Duration / (float)Settings.UpdatesPerSecond, 2) + Color.Grey + " seconds";
						break;
					case EffectType.NONE:
						continue;
					default:
						text += $" {direction} by factor " + Color.Magenta + Math.Round(effect.Value, 2) + Color.Grey + " for " + Color.Cyan + Math.Round(effect.Duration / (float)Settings.UpdatesPerSecond, 2) + Color.Grey + " seconds";
						break;
				}

				text += " | " + Color.Green + effect.Activation.ToString().Replace('_', ' ');

				array[3 + i] = text;
			}

			if (descCount > 0)
			{
				array[3 + effectCount] = Color.White + "Description: ";
				array[4 + effectCount] = Color.Grey + Description;
			}

			return array;
		}
	}
}
