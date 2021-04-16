using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	class SaveGameScreen : Screen
	{
		readonly Game game;

		readonly GameSaveList list;
		readonly NewSaveGameScreen createSaveScreen;

		public SaveGameScreen(Game game) : base("Save Game")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			list = new GameSaveList(new MPos((int)(WindowInfo.UnitWidth * 128), 4096), "wooden") { Position = new CPos(0, 1024, 0) };

			Content.Add(new Button("Back", "wooden", () => game.ShowScreen(ScreenType.MENU)) { Position = new CPos(4096, 6144, 0) });
			Content.Add(new Button("Save", "wooden", () => { saveGame(); game.RefreshSaveGameScreens(); }) { Position = new CPos(0, 6144, 0) });
			Content.Add(new Button("New Save", "wooden", () => createSaveScreen.ActiveScreen = true) { Position = new CPos(-4096, 6144, 0) });

			Content.Add(list);

			createSaveScreen = new NewSaveGameScreen(game);
		}

		public override void Hide()
		{
			list.DisableTooltip();
		}

		void saveGame()
		{
			var stats = list.GetStatistic();
			if (stats == null)
				return;

			var @new = game.Statistics.Copy();

			void action1()
			{
				game.ShowScreen(ScreenType.SAVEGAME);
			}
			void action2()
			{
				game.ShowScreen(ScreenType.MENU);
				GameSaveManager.SaveOnNewName(@new, stats.Name, game);
				game.AddInfoMessage(150, "Game Saved!");
				Log.WriteDebug("Overrided a game: " + stats.SaveName);
			}
			game.ShowDecisionScreen(action1, action2, "Are you sure you want to override?");
		}

		public void UpdateList()
		{
			list.Refresh();
		}

		public override void Render()
		{
			if (createSaveScreen.ActiveScreen)
				createSaveScreen.Render();
			else
				base.Render();
		}

		public override void Tick()
		{
			if (createSaveScreen.ActiveScreen)
				createSaveScreen.Tick();
			else
				base.Tick();
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.MENU);
		}
	}

	class NewSaveGameScreen : Screen
	{
		public bool ActiveScreen = false;

		readonly Game game;

		readonly TextBox @new;

		public NewSaveGameScreen(Game game) : base("New Save")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			var back = new Button("Back", "wooden", () => ActiveScreen = false) { Position = new CPos(4096, 6144, 0) };
			Content.Add(back);
			var create = new Button("Save", "wooden", save) { Position = new CPos(0, 6144, 0) };
			Content.Add(create);

			@new = new TextBox(game.Statistics.Name, "wooden", 20, isPath: true) { OnEnter = save };
			Content.Add(@new);

			var warning = new UITextLine(FontManager.Pixel16, TextOffset.MIDDLE) { Position = new CPos(0, 1024, 0) };
			warning.WriteText(Color.Red + "WARNING: " + Color.White + "You have to save over the just created save!");
			Content.Add(warning);
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.MENU);
		}

		void save()
		{
			ActiveScreen = false;
			GameSaveManager.SaveOnNewName(game.Statistics, @new.Text, game);

			game.RefreshSaveGameScreens();
			Log.WriteDebug("Saved a game: " + game.Statistics.SaveName);
		}
	}
}
