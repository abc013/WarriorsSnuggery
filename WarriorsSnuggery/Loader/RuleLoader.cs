using System;
using WarriorsSnuggery.UI;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	static class RuleLoader
	{
		public static void LoadRules()
		{
			var rules = RuleReader.Read(FileExplorer.Rules + "Rules.yaml");

			foreach (var rule in rules)
			{
				switch (rule.Key)
				{
					case "Particles":
						var particles = rule.ToArray();
						foreach (var particle in particles)
							ParticleCreator.LoadTypes(FileExplorer.FindIn(FileExplorer.Rules, particle, ".yaml"));
						break;
					case "Weapons":
						var weapons = rule.ToArray();
						foreach (var weapon in weapons)
							WeaponCreator.LoadTypes(FileExplorer.FindIn(FileExplorer.Rules, weapon, ".yaml"));
						break;
					case "Actors":
						var actors = rule.ToArray();
						foreach (var actor in actors)
							ActorCreator.LoadTypes(FileExplorer.FindIn(FileExplorer.Rules, actor, ".yaml"));
						break;
					case "Terrain":
						var terrain = rule.ToArray();
						foreach (var terrain2 in terrain)
							TerrainCreator.LoadTypes(FileExplorer.FindIn(FileExplorer.Rules, terrain2, ".yaml"));
						break;
					case "Walls":
						var walls = rule.ToArray();
						foreach (var wall in walls)
							WallCreator.LoadTypes(FileExplorer.FindIn(FileExplorer.Rules, wall, ".yaml"));
						break;
				}
			}
		}

		public static void LoadUIRules()
		{
			// PERF: put hover, click and normal all in one file and load as sprite to save load time and copy time;
			ButtonCreator.AddType(new ButtonType(0.5f, 2.5f, "UI_wood1", "UI_wood2", "UI_wood3", 4), "wooden");

			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("Check"), 1.5f), new ImageRenderable(TextureManager.Texture("CheckOn"), 1.5f), new ImageRenderable(TextureManager.Texture("CheckClick"), 1.5f), 1f, 1f, 5, ParticleCreator.GetType("woodPiece")), "wooden");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_terrain"), 1.5f), new ImageRenderable(TextureManager.Texture("check_terrain_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_terrain_click"), 1.5f), 0.5f, 0.5f, 5, ParticleCreator.GetType("woodPiece")), "terrain_editor");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_actor"), 1.5f), new ImageRenderable(TextureManager.Texture("check_actor_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_actor_click"), 1.5f), 0.5f, 0.5f, 5, ParticleCreator.GetType("woodPiece")), "actor_editor");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_object"), 1.5f), new ImageRenderable(TextureManager.Texture("check_object_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_object_click"), 1.5f), 0.5f, 0.5f, 5, ParticleCreator.GetType("woodPiece")), "object_editor");
			CheckBoxCreator.AddType(new CheckBoxType(new ImageRenderable(TextureManager.Texture("check_wall"), 1.5f), new ImageRenderable(TextureManager.Texture("check_wall_hover"), 1.5f), new ImageRenderable(TextureManager.Texture("check_wall_click"), 1.5f), 0.5f, 0.5f, 5, ParticleCreator.GetType("woodPiece")), "wall_editor");

			TextBoxCreator.AddType(new TextBoxType("UI_wood1", "UI_wood2", "UI_wood3", 4), "wooden");
		}
	}
}
