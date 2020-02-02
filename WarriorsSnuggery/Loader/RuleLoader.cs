using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI;

namespace WarriorsSnuggery
{
	static class RuleLoader
	{
		public static void LoadRules()
		{
			var rules = RuleReader.Read(FileExplorer.Rules, "Rules.yaml");
			var terrainFiles = new string[0];
			var terrainPaths = new string[0];

			var uiFiles = new string[0];
			var uiPaths = new string[0];

			SpriteManager.CreateSheet(5);

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
			SpriteManager.CreateTextures();

			TerrainSpriteManager.CreateSheet();

			for (int j = 0; j < terrainFiles.Length; j++)
				TerrainCreator.LoadTypes(terrainPaths[j], terrainFiles[j] + ".yaml");

			TerrainSpriteManager.CreateTexture();

			UISpriteManager.CreateSheet();

			for (int j = 0; j < uiFiles.Length; j++)
				Spells.SpellTreeLoader.Load(uiPaths[j], uiFiles[j] + ".yaml");
			loadUIRules();

			UISpriteManager.CreateTexture();
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

			PanelManager.AddType(new PanelType("UI_wood1", "UI_wood2", "UI_wood3", 4), "wooden");
			PanelManager.AddType(new PanelType("UI_stone1", "UI_wood3", "UI_stone2", 4), "stone");

			CheckBoxCreator.AddType(new CheckBoxType(UISpriteManager.AddTexture(checkBox("check"))[0], UISpriteManager.AddTexture(checkBox("check_hover"))[0], UISpriteManager.AddTexture(checkBox("check_click"))[0], 0.6f, 0.6f), "wooden");
			CheckBoxCreator.AddType(new CheckBoxType(UISpriteManager.AddTexture(checkBox("check_terrain"))[0], UISpriteManager.AddTexture(checkBox("check_terrain_hover"))[0], UISpriteManager.AddTexture(checkBox("check_terrain_click"))[0], 0.5f, 0.5f), "terrain_editor");
			CheckBoxCreator.AddType(new CheckBoxType(UISpriteManager.AddTexture(checkBox("check_actor"))[0], UISpriteManager.AddTexture(checkBox("check_actor_hover"))[0], UISpriteManager.AddTexture(checkBox("check_actor_click"))[0], 0.5f, 0.5f), "actor_editor");
			CheckBoxCreator.AddType(new CheckBoxType(UISpriteManager.AddTexture(checkBox("check_object"))[0], UISpriteManager.AddTexture(checkBox("check_object_hover"))[0], UISpriteManager.AddTexture(checkBox("check_object_click"))[0], 0.5f, 0.5f), "object_editor");
			CheckBoxCreator.AddType(new CheckBoxType(UISpriteManager.AddTexture(checkBox("check_wall"))[0], UISpriteManager.AddTexture(checkBox("check_wall_hover"))[0], UISpriteManager.AddTexture(checkBox("check_wall_click"))[0], 0.5f, 0.5f), "wall_editor");
		}

		static TextureInfo checkBox(string file)
		{
			return new TextureInfo(file, TextureType.IMAGE, 0, 10, 10);
		}
	}
}
