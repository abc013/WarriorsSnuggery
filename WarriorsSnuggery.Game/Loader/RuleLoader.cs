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
			var rules = TextNodeLoader.FromFile(FileExplorer.Rules, "Rules.yaml");

			var timer = Timer.Start();

			foreach (var rule in rules)
			{
				var data = new (string directory, string file)[rule.Children.Count];
				for (int i = 0; i < data.Length; i++)
				{
					var file = rule.Children[i].Key;
					data[i] = (FileExplorer.FindPath(FileExplorer.Rules, file, string.Empty), file);
				}

				var loader = new ComplexTextNodeLoader(rule.Key);
				loader.Load(data);

				var nodes = loader.Finish();
				switch (rule.Key)
				{
					case "Particles":
						ParticleCache.Load(nodes);

						break;
					case "Weapons":
						WeaponCache.Load(nodes);

						break;
					case "Actors":
						ActorCache.Load(nodes);

						break;
					case "Terrain":
						TerrainCache.Load(nodes);

						break;
					case "Walls":
						WallCache.Load(nodes);

						break;
					case "Spells":
						SpellCache.Load(nodes);

						break;
					case "SpellTree":
						SpellCasterCache.Load(nodes);

						break;
					case "Trophies":
						TrophyCache.Load(nodes);

						break;
					case "Maps":
						MapCache.Load(nodes);

						break;
				}

				timer.StopAndWrite($"Loading Rules: {rule.Key}");
				timer.Restart();
			}

			ShroudTexture = new TextureInfo("shroud").GetTextures();

			Questionmark = new TextureInfo("questionmark").GetTextures();

			loadUIRules();

			timer.StopAndWrite("Loading UI Rules");
		}

		static void loadUIRules()
		{
			UISpriteManager.Add("UI_inactiveConnection", new TextureInfo("UI_inactiveConnection", TextureType.ANIMATION, 5, 3, 10));
			UISpriteManager.Add("UI_activeConnection", new TextureInfo("UI_activeConnection", TextureType.ANIMATION, 5, 3, 10));
			UISpriteManager.Add("UI_save", "UI_save");
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
