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
		public static Texture ShroudTexture;
		public static Texture BigShroudTexture;
		public static Texture Questionmark;

		public static void Load()
		{
			var timer = Timer.Start();

			ParticleCache.Load(loadNodes("Particles"));
			EffectCache.Load(loadNodes("Spelleffects"));
			WeaponCache.Load(loadNodes("Weapons"));
			ActorCache.Load(loadNodes("Actors"));
			TerrainCache.Load(loadNodes("Terrain"));
			WallCache.Load(loadNodes("Walls"));
			SpellCasterCache.Load(loadNodes("Spellcasters"));
			TrophyCache.Load(loadNodes("Trophies"));
			MapCache.Load(loadNodes("Maps"));

			timer.StopAndWrite($"Loading Game Rules");
			timer.Restart();

			ShroudTexture = new TextureInfo(new PackageFile("shroud")).GetTextures()[0];
			BigShroudTexture = new TextureInfo(new PackageFile("bigshroud")).GetTextures()[0];
			Questionmark = new TextureInfo(new PackageFile("questionmark")).GetTextures()[0];

			loadUIRules();

			timer.StopAndWrite("Loading UI Rules");
		}

		static List<TextNode> loadNodes(string rule)
		{
			var loader = new ComplexTextNodeLoader(rule);

			foreach (var node in getFiles(rule))
			{
				var packageFile = new PackageFile(node.Key);
				loader.Load(FileExplorer.FindPath(packageFile.Package.RulesDirectory, FileExplorer.FileName(packageFile.File), FileExplorer.FileExtension(packageFile.File)), packageFile.File);
			}

			return loader.Finish();
		}

		static List<TextNode> getFiles(string rule)
		{
			var list = new List<TextNode>();

			foreach (var package in PackageManager.ActivePackages)
			{
				var textNode = package.Rules.FirstOrDefault(n => n.Key == rule);
				if (textNode != null)
					list.AddRange(textNode.Children);
			}

			return list;
		}

		static void loadUIRules()
		{
			UISpriteManager.Add("UI_inactiveConnection", new TextureInfo(new PackageFile("UI_inactiveConnection"), TextureType.ANIMATION, 5, 3, 10));
			UISpriteManager.Add("UI_activeConnection", new TextureInfo(new PackageFile("UI_activeConnection"), TextureType.ANIMATION, 5, 3, 10));
			UISpriteManager.Add(new PackageFile("UI_save"));
			UISpriteManager.Add(new PackageFile("UI_gear"));
			UISpriteManager.Add(new PackageFile("UI_map"));
			UISpriteManager.Add(new PackageFile("UI_money"));
			UISpriteManager.Add(new PackageFile("UI_key"));
			UISpriteManager.Add(new PackageFile("UI_heart"));
			UISpriteManager.Add("keyboard", new TextureInfo(new PackageFile("keyboard"), TextureType.ANIMATION, 24, 24));
			UISpriteManager.Add(new PackageFile("UI_selector1"));
			UISpriteManager.Add(new PackageFile("UI_selector2"));
			UISpriteManager.Add(new PackageFile("UI_enemy_arrow"));
			UISpriteManager.Add(new PackageFile("cursor_default"));
			UISpriteManager.Add(new PackageFile("cursor_select"));
			UISpriteManager.Add(new PackageFile("cursor_money"));
			UISpriteManager.Add(new PackageFile("cursor_attack"));
			UISpriteManager.Add(new PackageFile("logo"));

			PanelCache.Add(panel(new PackageFile("UI_wood1"), new PackageFile("UI_wood2"), new PackageFile("UI_wood3"), 72), "wooden");
			PanelCache.Add(panel(new PackageFile("UI_stone1"), new PackageFile("UI_wood3"), new PackageFile("UI_stone2"), 72), "stone");

			CheckBoxCache.Add(checkBox(new PackageFile("check")), "wooden");

			CheckBoxCache.Add(checkBox(new PackageFile("check_terrain")), "terrain_editor");
			CheckBoxCache.Add(checkBox(new PackageFile("check_actor")), "actor_editor");
			CheckBoxCache.Add(checkBox(new PackageFile("check_object")), "object_editor");
			CheckBoxCache.Add(checkBox(new PackageFile("check_wall")), "wall_editor");

			CheckBoxCache.Add(checkBox(new PackageFile("check_menu")), "menu");

			foreach (var sound in new [] { new PackageFile("money_spent1"), new PackageFile("money_spent2"), new PackageFile("money_spent3") })
				AudioManager.LoadSound(sound);

			AudioManager.LoadSound(new PackageFile("click"));
			AudioManager.LoadSound(new PackageFile("ping"));

			AudioManager.LoadSound(new PackageFile("life_lost"));
		}

		static CheckBoxType checkBox(PackageFile packageFile)
		{
			var hoverFile = new PackageFile(packageFile.Package, packageFile.File + "_hover");
			var clickFile = new PackageFile(packageFile.Package, packageFile.File + "_click");
			return new CheckBoxType(getTexture(packageFile), getTexture(hoverFile), getTexture(clickFile));
		}

		static PanelType panel(PackageFile bgFile, PackageFile bg2File, PackageFile borderFile, int borderWidth)
		{
			return new PanelType(getTexture(bgFile), getTexture(bg2File), getTexture(borderFile), borderWidth);
		}

		static Texture getTexture(PackageFile packageFile)
		{
			return new TextureInfo(packageFile).GetTextures()[0];
		}
	}
}
