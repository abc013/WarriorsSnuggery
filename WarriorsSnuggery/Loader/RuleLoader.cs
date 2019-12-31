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
						for (int j = 0; j < files.Length; j++)
							Spells.SpellTreeLoader.Load(paths[j], files[j] + ".yaml");

						break;
				}
			}
			SpriteManager.CreateTextures();

			TerrainSpriteManager.CreateSheet();

			for (int j = 0; j < terrainFiles.Length; j++)
				TerrainCreator.LoadTypes(terrainPaths[j], terrainFiles[j] + ".yaml");

			TerrainSpriteManager.CreateTexture();
		}

		public static void LoadUIRules()
		{
			PanelManager.AddType(new PanelType("UI_wood1", "UI_wood2", "UI_wood3", 4), "wooden");
			PanelManager.AddType(new PanelType("UI_stone1", "UI_wood3", "UI_stone2", 4), "stone");

			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("Check"), 1.5f), new ImageRenderable(TextureManager.Texture("CheckOn"), 1.5f), new ImageRenderable(TextureManager.Texture("CheckClick"), 1.5f), 0.6f, 0.6f), "wooden");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_terrain"), 1.5f), new ImageRenderable(TextureManager.Texture("check_terrain_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_terrain_click"), 1.5f), 0.5f, 0.5f), "terrain_editor");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_actor"), 1.5f), new ImageRenderable(TextureManager.Texture("check_actor_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_actor_click"), 1.5f), 0.5f, 0.5f), "actor_editor");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_object"), 1.5f), new ImageRenderable(TextureManager.Texture("check_object_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_object_click"), 1.5f), 0.5f, 0.5f), "object_editor");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_wall"), 1.5f), new ImageRenderable(TextureManager.Texture("check_wall_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_wall_click"), 1.5f), 0.5f, 0.5f), "wall_editor");
		}
	}
}
