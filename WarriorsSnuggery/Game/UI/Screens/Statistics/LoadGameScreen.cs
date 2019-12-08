using System;

namespace WarriorsSnuggery.UI
{
	class LoadGameScreen : Screen
	{
		readonly Game game;

		readonly GameSaveList list;

		public LoadGameScreen(Game game) : base("Load Game")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			list = new GameSaveList(new CPos(0, 1024, 0), new MPos((int)(WindowInfo.UnitWidth * 128), 4096), PanelManager.Get("wooden"), "UI_save");

			Content.Add(ButtonCreator.Create("wooden", new CPos(4096, 6144, 0), "Back", () => game.ChangeScreen(ScreenType.MENU)));
			void loadAction()
			{
				var stats = list.GetStatistic();
				if (stats != null)
				{
					humanAgreeOnLoad(() =>
					{
						GameController.CreateNew(new GameStatistics(stats), loadStatsMap: true);
					}, "Are you sure to leave this game? Unsaved progress will be lost!");
				}
			}
			Content.Add(ButtonCreator.Create("wooden", new CPos(0, 6144, 0), "Load", loadAction));
			void deleteAction()
			{
				var stats = list.GetStatistic();
				if (stats != null)
				{
					humanAgreeOnDelete(() =>
					{
						GameSaveManager.Delete(stats);
						game.RefreshSaveGameScreens();
						game.ScreenControl.ShowScreen(ScreenType.LOAD);
					}, "Are you sure to delete this save?");
				}
			}
			Content.Add(ButtonCreator.Create("wooden", new CPos(-4096, 6144, 0), "Delete", deleteAction));
		}

		public override void Hide()
		{
			list.DisableTooltip();
		}

		void humanAgreeOnLoad(Action onAgree, string text)
		{
			if (game.Type == GameType.MAINMENU)
				onAgree();

			void onDecline()
			{
				game.ScreenControl.ShowScreen(ScreenType.LOAD);
			}
			game.ScreenControl.SetDecision(onDecline, onAgree, text);
			game.ScreenControl.ShowScreen(ScreenType.DECISION);
		}

		void humanAgreeOnDelete(Action onAgree, string text)
		{
			void onDecline()
			{
				game.ScreenControl.ShowScreen(ScreenType.LOAD);
			}

			game.ScreenControl.SetDecision(onDecline, onAgree, text);
			game.ScreenControl.ShowScreen(ScreenType.DECISION);
		}

		public void UpdateList()
		{
			list.Refresh();
		}

		public override void Render()
		{
			base.Render();

			list.Render();
		}

		public override void Tick()
		{
			base.Tick();

			list.Tick();

			if (KeyInput.IsKeyDown("escape", 10))
				game.ChangeScreen(ScreenType.MENU);

		}

		public override void Dispose()
		{
			base.Dispose();
			list.Dispose();
		}
	}
}
