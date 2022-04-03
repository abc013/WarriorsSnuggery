using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Spells;
using WarriorsSnuggery.Trophies;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.Loader
{
	public static class RuleLoader
	{
		public static Texture[] ShroudTexture;
		public static Texture[] Questionmark;

		public static void Load()
		{
			var timer = Timer.Start();

			ParticleCache.Load(loadNodes("Particles"));
			SpellCache.Load(loadNodes("Spells"));
			WeaponCache.Load(loadNodes("Weapons"));
			ActorCache.Load(loadNodes("Actors"));
			TerrainCache.Load(loadNodes("Terrain"));
			WallCache.Load(loadNodes("Walls"));
			SpellCasterCache.Load(loadNodes("SpellTree"));
			TrophyCache.Load(loadNodes("Trophies"));
			MapCache.Load(loadNodes("Maps"));

			timer.StopAndWrite($"Loading Game Rules");
			timer.Restart();

			ShroudTexture = new TextureInfo("shroud").GetTextures();
			Questionmark = new TextureInfo("questionmark").GetTextures();

			loadUIRules();

			timer.StopAndWrite("Loading UI Rules");
		}

		static List<TextNode> loadNodes(string rule)
		{
			var loader = new ComplexTextNodeLoader(rule);

			foreach (var node in getFiles(rule))
			{
				var file = node.Key;
				loader.Load(FileExplorer.FindPath(FileExplorer.ResolveMod(file).RulesDirectory, FileExplorer.FileName(FileExplorer.ResolveFile(file)), FileExplorer.FileExtension(FileExplorer.ResolveFile(file))), FileExplorer.ResolveFile(file));
			}

			return loader.Finish();
		}

		static List<TextNode> getFiles(string rule)
		{
			var list = new List<TextNode>();

			foreach (var mod in ModManager.ActiveMods)
			{
				var textNode = mod.Rules.FirstOrDefault(n => n.Key == rule);
				if (textNode != null)
					list.AddRange(textNode.Children);
			}

			return list;
		}

		static void loadUIRules()
		{
			UISpriteManager.Add("UI_inactiveConnection", new TextureInfo("UI_inactiveConnection", TextureType.ANIMATION, 5, 3, 10));
			UISpriteManager.Add("UI_activeConnection", new TextureInfo("UI_activeConnection", TextureType.ANIMATION, 5, 3, 10));
			UISpriteManager.Add("UI_save", "UI_save");
			UISpriteManager.Add("UI_gear", "UI_gear");
			UISpriteManager.Add("UI_map", "UI_map");
			UISpriteManager.Add("UI_money", "UI_money");
			UISpriteManager.Add("UI_key", "UI_key");
			UISpriteManager.Add("UI_heart", "UI_heart");
			UISpriteManager.Add("keyboard", new TextureInfo("keyboard", TextureType.ANIMATION, 24, 24));
			UISpriteManager.Add("UI_selector1", "UI_selector1");
			UISpriteManager.Add("UI_selector2", "UI_selector2");
			UISpriteManager.Add("UI_enemy_arrow", "UI_enemy_arrow");
			UISpriteManager.Add("cursor_default", "cursor_default");
			UISpriteManager.Add("cursor_select", "cursor_select");
			UISpriteManager.Add("cursor_money", "cursor_money");
			UISpriteManager.Add("cursor_attack", "cursor_attack");
			UISpriteManager.Add("logo", "logo");

			PanelCache.Add(new PanelType(getTexture("UI_wood1"), getTexture("UI_wood2"), getTexture("UI_wood3"), 72), "wooden");
			PanelCache.Add(new PanelType(getTexture("UI_stone1"), getTexture("UI_wood3"), getTexture("UI_stone2"), 72), "stone");

			CheckBoxCache.Add(checkBox("check"), "wooden");

			CheckBoxCache.Add(checkBox("check_terrain"), "terrain_editor");
			CheckBoxCache.Add(checkBox("check_actor"), "actor_editor");
			CheckBoxCache.Add(checkBox("check_object"), "object_editor");
			CheckBoxCache.Add(checkBox("check_wall"), "wall_editor");

			CheckBoxCache.Add(checkBox("check_menu"), "menu");

			foreach (var sound in new [] { "money_spent1", "money_spent2", "money_spent3" })
				AudioManager.LoadSound(sound, FileExplorer.FindPath(FileExplorer.ResolveMod(sound).ContentDirectory, FileExplorer.ResolveFile(sound), ".wav"));

			AudioManager.LoadSound("click", FileExplorer.FindPath(ModManager.Core.ContentDirectory, "click", ".wav"));
			AudioManager.LoadSound("ping", FileExplorer.FindPath(ModManager.Core.ContentDirectory, "ping", ".wav"));

			AudioManager.LoadSound("life_lost", FileExplorer.FindPath(ModManager.Core.ContentDirectory, "life_lost", ".wav"));
		}

		static CheckBoxType checkBox(string name)
		{
			return new CheckBoxType(getTexture(name), getTexture(name + "_hover"), getTexture(name + "_click"));
		}

		static Texture getTexture(string file)
		{
			return new TextureInfo(file).GetTextures()[0];
		}
	}
}
