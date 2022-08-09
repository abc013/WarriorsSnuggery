using OpenTK.Windowing.GraphicsLibraryFramework;
using WarriorsSnuggery.UI.Objects;

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
			Title.Position = new UIPos(0, -4096);

			list = new GameSaveList(4096, "wooden") { Position = new UIPos(0, 1024) };

			Add(new Button("Back", "wooden", () => game.ShowScreen(ScreenType.MENU)) { Position = new UIPos(4096, 6144) });
			Add(new Button("Save", "wooden", () => { saveGame(); game.RefreshSaveGameScreens(); }) { Position = new UIPos(0, 6144) });
			Add(new Button("New Save", "wooden", () => createSaveScreen.ActiveScreen = true) { Position = new UIPos(-4096, 6144) });

			Add(list);

			createSaveScreen = new NewSaveGameScreen(game);
		}

		public override void Hide()
		{
			list.DisableTooltip();
		}

		void saveGame()
		{
			var save = list.SelectedSave;
			if (save == null)
				return;

			var @new = game.Save.Copy();

			void action1()
			{
				game.ShowScreen(ScreenType.SAVEGAME);
			}
			void action2()
			{
				game.ShowScreen(ScreenType.MENU);
				GameSaveManager.SaveOnNewName(@new, save.Name, game);
				game.AddInfoMessage(150, "Game Saved!");
				Log.Debug($"Overrided game save ({@new.Name}->{save.SaveName}).");
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
			if (createSaveScreen.ActiveScreen)
			{
				createSaveScreen.KeyDown(key, isControl, isShift, isAlt);
				return;
			}

			base.KeyDown(key, isControl, isShift, isAlt);

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
			Title.Position = new UIPos(0, -4096);

			var back = new Button("Back", "wooden", () => ActiveScreen = false) { Position = new UIPos(4096, 6144) };
			Add(back);
			var create = new Button("Save", "wooden", save) { Position = new UIPos(0, 6144) };
			Add(create);

			@new = new TextBox("wooden", 20, InputType.PATH)
			{
				Text = game.Save.Name,
				OnEnter = save
			};
			Add(@new);
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			base.KeyDown(key, isControl, isShift, isAlt);

			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.MENU);
		}

		void save()
		{
			if (@new.Text == GameSaveManager.DefaultSaveName)
				return;

			ActiveScreen = false;
			GameSaveManager.SaveOnNewName(game.Save, @new.Text, game);

			game.RefreshSaveGameScreens();
			Log.Debug($"Saved new game save '{game.Save.SaveName}'.");
		}
	}
}
