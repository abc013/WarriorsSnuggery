namespace WarriorsSnuggery.UI
{
	class LoadGameScreen : Screen
	{
		readonly Game game;

		readonly GameSaveList list;
		readonly Button back;
		readonly Button load;
		readonly Button delete;

		public LoadGameScreen(Game game) : base("Load Game")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			list = new GameSaveList(new CPos(0, 1024, 0), new MPos((int)(WindowInfo.UnitWidth * 128), 4096), 5, "UI_wood1", "UI_save", "UI_wood3", "UI_wood2");

			back = ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Back", () => game.ChangeScreen(ScreenType.MENU));
			load = ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "Load", () => { if(list.GetStatistic() != null) Window.Current.NewGame(new GameStatistics(list.GetStatistic())); });
			delete = ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Delete", () => { if (list.GetStatistic() != null) { GameSaveManager.Delete(list.GetStatistic()); game.RefreshSaveGameScreens(); } });
		}

		public void UpdateList()
		{
			list.Refresh();
		}

		public override void Render()
		{
			base.Render();

			back.Render();
			load.Render();
			delete.Render();
			list.Render();
		}

		public override void Tick()
		{
			base.Tick();

			back.Tick();
			load.Tick();
			delete.Tick();
			list.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
			{
				game.ChangeScreen(ScreenType.MENU);
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			list.Dispose();

			back.Dispose();
			load.Dispose();
			delete.Dispose();
		}
	}
}
