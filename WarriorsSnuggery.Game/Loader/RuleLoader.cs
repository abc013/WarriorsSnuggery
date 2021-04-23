using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Trophies;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.Loader
{
	public static class RuleLoader
	{
		public static Texture[] ShroudTexture;
		public static Texture[] Questionmark;

		public static void LoadRules()
		{
			var rules = TextNodeLoader.FromFile(FileExplorer.Rules, "Rules.yaml");

			foreach (var rule in rules)
			{
				var files = rule.Convert<string[]>();
				var paths = new string[files.Length];
				for (int i = 0; i < paths.Length; i++)
					paths[i] = FileExplorer.FindPath(FileExplorer.Rules, files[i], string.Empty);

				switch (rule.Key)
				{
					case "Particles":
						for (int j = 0; j < files.Length; j++)
							ParticleCreator.Load(paths[j], files[j]);

						break;
					case "Weapons":
						for (int j = 0; j < files.Length; j++)
							WeaponCreator.Load(paths[j], files[j]);

						break;
					case "Actors":
						for (int j = 0; j < files.Length; j++)
							ActorCreator.Load(paths[j], files[j]);

						break;
					case "Terrain":
						for (int j = 0; j < files.Length; j++)
							TerrainCreator.LoadTypes(paths[j], files[j]);

						break;
					case "Walls":
						for (int j = 0; j < files.Length; j++)
							WallCreator.Load(paths[j], files[j]);

						break;
					case "Spells":
						for (int j = 0; j < files.Length; j++)
							Spells.SpellCreator.Load(paths[j], files[j]);

						break;
					case "SpellTree":
						for (int j = 0; j < files.Length; j++)
							Spells.SpellTreeLoader.Load(paths[j], files[j]);

						break;
					case "Trophies":
						for (int j = 0; j < files.Length; j++)
							TrophyManager.Load(paths[j], files[j]);

						break;
				}
			}

			ShroudTexture = new TextureInfo("shroud").GetTextures();

			Questionmark = new TextureInfo("questionmark").GetTextures();

			loadUIRules();
		}

		static void loadUIRules()
		{
			UISpriteManager.Add("UI_inactiveConnection", new TextureInfo("UI_inactiveConnection", TextureType.ANIMATION, 10, 5, 3));
			UISpriteManager.Add("UI_activeConnection", new TextureInfo("UI_activeConnection", TextureType.ANIMATION, 10, 5, 3));
			UISpriteManager.Add("UI_save", "UI_save");
			UISpriteManager.Add("UI_map", "UI_map");
			UISpriteManager.Add("UI_money", "UI_money");
			UISpriteManager.Add("UI_key", "UI_key");
			UISpriteManager.Add("keyboard", new TextureInfo("keyboard", TextureType.ANIMATION, 10, 24, 24));
			UISpriteManager.Add("UI_selector1", "UI_selector1");
			UISpriteManager.Add("UI_selector2", "UI_selector2");
			UISpriteManager.Add("UI_enemy_arrow", "UI_enemy_arrow");
			UISpriteManager.Add("cursor_default", "cursor_default");
			UISpriteManager.Add("cursor_select", "cursor_select");
			UISpriteManager.Add("cursor_money", "cursor_money");
			UISpriteManager.Add("cursor_attack", "cursor_attack");
			UISpriteManager.Add("logo", "logo");

			PanelManager.AddType(new PanelType(getTexture("UI_wood1"), getTexture("UI_wood2"), getTexture("UI_wood3"), 72), "wooden");
			PanelManager.AddType(new PanelType(getTexture("UI_stone1"), getTexture("UI_wood3"), getTexture("UI_stone2"), 72), "stone");

			CheckBoxManager.AddType(checkBox("check"), "wooden");

			CheckBoxManager.AddType(checkBox("check_terrain"), "terrain_editor");
			CheckBoxManager.AddType(checkBox("check_actor"), "actor_editor");
			CheckBoxManager.AddType(checkBox("check_object"), "object_editor");
			CheckBoxManager.AddType(checkBox("check_wall"), "wall_editor");

			CheckBoxManager.AddType(checkBox("check_menu"), "menu");
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
