using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.UI;

namespace WarriorsSnuggery
{
	static class RuleLoader
	{
		public static void LoadRules()
		{
			var rules = RuleReader.Read(FileExplorer.Rules, "Rules.yaml"); //TODO for future: use for mods
			string[] terrain = null;

			SpriteManager.CreateSheet(7);
			foreach (var rule in rules)
			{
				switch (rule.Key)
				{
					case "Particles":
						var particles = rule.Convert<string[]>();
						foreach (var particle in particles)
							ParticleCreator.LoadTypes(FileExplorer.FindPath(FileExplorer.Rules, particle, ".yaml"), particle + ".yaml");
						break;
					case "Weapons":
						var weapons = rule.Convert<string[]>();
						foreach (var weapon in weapons)
							WeaponCreator.LoadTypes(FileExplorer.FindPath(FileExplorer.Rules, weapon, ".yaml"), weapon + ".yaml");
						break;
					case "Actors":
						var actors = rule.Convert<string[]>();
						foreach (var actor in actors)
							ActorCreator.LoadTypes(FileExplorer.FindPath(FileExplorer.Rules, actor, ".yaml"), actor + ".yaml");

						break;
					case "Terrain":
						
						terrain = rule.Convert<string[]>();
						break;
					case "Walls":
						var walls = rule.Convert<string[]>();
						foreach (var wall in walls)
							WallCreator.LoadTypes(FileExplorer.FindPath(FileExplorer.Rules, wall, ".yaml"), wall + ".yaml");
						break;
					case "TechTree":
						var trees = rule.Convert<string[]>();
						foreach (var tree in trees)
							TechTreeLoader.LoadTechTree(tree);
						break;
				}
			}
			SpriteManager.CreateTextures();

			TerrainSpriteManager.CreateSheet();

			foreach (var terrain2 in terrain)
				TerrainCreator.LoadTypes(FileExplorer.FindPath(FileExplorer.Rules, terrain2, ".yaml"), terrain2 + ".yaml");

			TerrainSpriteManager.CreateTexture();
		}

		public static void LoadUIRules()
		{
			// PERF: put hover, click and normal all in one file and load as sprite to save load time and copy time;
			ButtonCreator.AddType(new PanelType(0.5f, 2.5f, "UI_wood1", "UI_wood2", "UI_wood3", 4), "wooden");

			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("Check"), 1.5f), new ImageRenderable(TextureManager.Texture("CheckOn"), 1.5f), new ImageRenderable(TextureManager.Texture("CheckClick"), 1.5f), 0.6f, 0.6f), "wooden");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_terrain"), 1.5f), new ImageRenderable(TextureManager.Texture("check_terrain_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_terrain_click"), 1.5f), 0.5f, 0.5f), "terrain_editor");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_actor"), 1.5f), new ImageRenderable(TextureManager.Texture("check_actor_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_actor_click"), 1.5f), 0.5f, 0.5f), "actor_editor");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_object"), 1.5f), new ImageRenderable(TextureManager.Texture("check_object_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_object_click"), 1.5f), 0.5f, 0.5f), "object_editor");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_wall"), 1.5f), new ImageRenderable(TextureManager.Texture("check_wall_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_wall_click"), 1.5f), 0.5f, 0.5f), "wall_editor");

			TextBoxCreator.AddType(new PanelType(0f, 0f, "UI_wood1", "UI_wood2", "UI_wood3", 4), "wooden");
		}
	}
}
