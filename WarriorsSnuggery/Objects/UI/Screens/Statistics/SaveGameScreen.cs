using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
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

			list = new GameSaveList(new CPos(0, 1024, 0), new MPos((int)(WindowInfo.UnitWidth * 128), 4096), PanelManager.Get("wooden"));

			Content.Add(new Button(new CPos(4096, 6144, 0), "Back", "wooden", () => game.ChangeScreen(ScreenType.MENU)));
			Content.Add(new Button(new CPos(0, 6144, 0), "Save", "wooden", () => { saveGame(); game.RefreshSaveGameScreens(); }));
			Content.Add(new Button(new CPos(-4096, 6144, 0), "New Save", "wooden", () => createSaveScreen.ActiveScreen = true));

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
				game.ScreenControl.ShowScreen(ScreenType.SAVE);
			}
			void action2()
			{
				game.ScreenControl.ShowScreen(ScreenType.MENU);
				GameSaveManager.SaveOnNewName(@new, stats.Name, game);
				game.AddInfoMessage(150, "Game Saved!");
				Log.WriteDebug("Overrided a game: " + stats.SaveName);
			}
			game.ScreenControl.SetDecision(action1, action2, "Are you sure you want to override?");
			game.ScreenControl.ShowScreen(ScreenType.DECISION);
		}

		public void UpdateList()
		{
			list.Refresh();
		}

		public override void Render()
		{
			if (createSaveScreen.ActiveScreen)
			{
				createSaveScreen.Render();
				return;
			}
			base.Render();
		}

		public override void Tick()
		{
			if (createSaveScreen.ActiveScreen)
			{
				createSaveScreen.Tick();
				return;
			}
			base.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
				game.ChangeScreen(ScreenType.MENU);
		}
	}

	class NewSaveGameScreen : Screen
	{
		public bool ActiveScreen = false;

		readonly Game game;

		readonly Button back;
		readonly Button create;
		readonly TextBox @new;
		readonly UITextLine warning;

		public NewSaveGameScreen(Game game) : base("New Save")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			back = new Button(new CPos(4096, 6144, 0), "Back", "wooden", () => ActiveScreen = false);
			create = new Button(new CPos(0, 6144, 0), "Save", "wooden", save);
			@new = new TextBox(CPos.Zero, "Name", "wooden", 20, isPath: true)
			{
				OnEnter = save
			};

			warning = new UITextLine(new CPos(0, 1024, 0), FontManager.Pixel16, TextOffset.MIDDLE);
			warning.WriteText(Color.Red + "WARNING: " + Color.White + "You have to save over the just created save!");
		}

		public override void Tick()
		{
			base.Tick();

			back.Tick();
			create.Tick();
			@new.Tick();
			warning.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
				game.ChangeScreen(ScreenType.MENU);
		}

		public override void Render()
		{
			base.Render();

			back.Render();
			create.Render();
			@new.Render();
			warning.Render();
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
