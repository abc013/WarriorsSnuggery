using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.UI;

namespace WarriorsSnuggery
{
	static class RuleLoader
	{
		public static ITexture[] ShroudTexture;
		public static ITexture[] Questionmark;

		public static void LoadRules()
		{
			var rules = RuleReader.Read(FileExplorer.Rules, "Rules.yaml");
			var terrainFiles = new string[0];
			var terrainPaths = new string[0];

			var uiFiles = new string[0];
			var uiPaths = new string[0];

			foreach (var rule in rules)
			{
				var files = rule.Convert<string[]>();
				var paths = new string[files.Length];
				for (int i = 0; i < paths.Length; i++)
					paths[i] = FileExplorer.FindPath(FileExplorer.Rules, files[i], ".yaml");

				switch (rule.Key)
				{
					case "Particles":
						for (int j = 0; j < files.Length; j++)
							ParticleCreator.Load(paths[j], files[j] + ".yaml");

						break;
					case "Weapons":
						for (int j = 0; j < files.Length; j++)
							WeaponCreator.Load(paths[j], files[j] + ".yaml");

						break;
					case "Actors":
						for (int j = 0; j < files.Length; j++)
							ActorCreator.Load(paths[j], files[j] + ".yaml");

						break;
					case "Terrain":
						terrainFiles = files;
						terrainPaths = paths;

						break;
					case "Walls":
						for (int j = 0; j < files.Length; j++)
							WallCreator.Load(paths[j], files[j] + ".yaml");

						break;
					case "Spells":
						uiFiles = files;
						uiPaths = paths;

						break;
				}
			}

			ShroudTexture = SpriteManager.AddTexture(new TextureInfo("shroud", TextureType.IMAGE, 0, 16, 16));
			Questionmark = SpriteManager.AddTexture(new TextureInfo("questionmark", TextureType.IMAGE, 0, 12, 12));

			for (int j = 0; j < terrainFiles.Length; j++)
				TerrainCreator.LoadTypes(terrainPaths[j], terrainFiles[j] + ".yaml");

			for (int j = 0; j < uiFiles.Length; j++)
				Spells.SpellTreeLoader.Load(uiPaths[j], uiFiles[j] + ".yaml");
			loadUIRules();
		}

		static void loadUIRules()
		{
			UITextureManager.Add("UI_inactiveConnection", new TextureInfo("UI_inactiveConnection", TextureType.ANIMATION, 10, 5, 3));
			UITextureManager.Add("UI_activeConnection", new TextureInfo("UI_activeConnection", TextureType.ANIMATION, 10, 5, 3));
			UITextureManager.Add("UI_save", new TextureInfo("UI_save", TextureType.IMAGE, 0, 16, 16));
			UITextureManager.Add("UI_map", new TextureInfo("UI_map", TextureType.IMAGE, 0, 16, 16));
			UITextureManager.Add("UI_money", new TextureInfo("UI_money", TextureType.IMAGE, 0, 21, 16));
			UITextureManager.Add("keyboard", new TextureInfo("keyboard", TextureType.ANIMATION, 0, 24, 24));
			UITextureManager.Add("UI_selector1", new TextureInfo("UI_selector1", TextureType.IMAGE, 0, 16, 16));
			UITextureManager.Add("UI_selector2", new TextureInfo("UI_selector2", TextureType.IMAGE, 0, 16, 16));
			UITextureManager.Add("cursor_default", new TextureInfo("cursor_default", TextureType.IMAGE, 0, 10, 10));
			UITextureManager.Add("cursor_select", new TextureInfo("cursor_select", TextureType.IMAGE, 0, 10, 10));
			UITextureManager.Add("cursor_money", new TextureInfo("cursor_money", TextureType.IMAGE, 0, 10, 10));
			UITextureManager.Add("cursor_attack", new TextureInfo("cursor_attack", TextureType.IMAGE, 0, 10, 10));

			PanelManager.AddType(new PanelType(panelTex("UI_wood1"), panelTex("UI_wood2"), panelTex("UI_wood3"), 0.1f), "wooden");
			PanelManager.AddType(new PanelType(panelTex("UI_stone1"), panelTex("UI_wood3"), panelTex("UI_stone2"), 0.1f), "stone");

			CheckBoxCreator.AddType(new CheckBoxType(checkBox("check"), checkBox("check_hover"), checkBox("check_click"), 0.6f, 0.6f), "wooden");
			CheckBoxCreator.AddType(new CheckBoxType(checkBox("check_terrain"), checkBox("check_terrain_hover"), checkBox("check_terrain_click"), 0.5f, 0.5f), "terrain_editor");
			CheckBoxCreator.AddType(new CheckBoxType(checkBox("check_actor"), checkBox("check_actor_hover"), checkBox("check_actor_click"), 0.5f, 0.5f), "actor_editor");
			CheckBoxCreator.AddType(new CheckBoxType(checkBox("check_object"), checkBox("check_object_hover"), checkBox("check_object_click"), 0.5f, 0.5f), "object_editor");
			CheckBoxCreator.AddType(new CheckBoxType(checkBox("check_wall"), checkBox("check_wall_hover"), checkBox("check_wall_click"), 0.5f, 0.5f), "wall_editor");
		}

		static ITexture checkBox(string file)
		{
			return SpriteManager.AddTexture(new TextureInfo(file, TextureType.IMAGE, 0, 10, 10))[0];
		}

		static ITexture panelTex(string file)
		{
			return SpriteManager.AddTexture(new TextureInfo(file, TextureType.IMAGE, 0, 16, 16))[0];
		}
	}
}
