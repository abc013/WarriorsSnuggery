﻿using OpenTK.Windowing.GraphicsLibraryFramework;
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

			list = new GameSaveList(new CPos(0, 1024, 0), new MPos((int)(WindowInfo.UnitWidth * 128), 4096), PanelManager.Get("wooden"));

			Content.Add(new Button(new CPos(4096, 6144, 0), "Back", "wooden", () => game.ChangeScreen(ScreenType.MENU)));
			void loadAction()
			{
				var stats = list.GetStatistic();
				if (stats != null)
				{
					humanAgreeOnLoad(() =>
					{
						Log.WriteDebug("Loading a game save: " + stats.SaveName);
						GameController.CreateNew(new GameStatistics(stats), loadStatsMap: true);
					}, "Are you sure to leave this game? Unsaved progress will be lost!");
				}
			}
			Content.Add(new Button(new CPos(0, 6144, 0), "Load", "wooden", loadAction));
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
						Log.WriteDebug("Deleting a game save: " + stats.SaveName);
					}, "Are you sure to delete this save?");
				}
			}
			Content.Add(new Button(new CPos(-4096, 6144, 0), "Delete", "wooden", deleteAction));

			Content.Add(list);
		}

		public override void Hide()
		{
			list.DisableTooltip();
		}

		void humanAgreeOnLoad(Action onAgree, string text)
		{
			if (game.Type == GameType.MAINMENU)
				onAgree();
			game.ScreenControl.SetDecision(onDecline, onAgree, text);
			game.ScreenControl.ShowScreen(ScreenType.DECISION);
		}

		void humanAgreeOnDelete(Action onAgree, string text)
		{
			game.ScreenControl.SetDecision(onDecline, onAgree, text);
			game.ScreenControl.ShowScreen(ScreenType.DECISION);
		}

		void onDecline()
		{
			game.ScreenControl.ShowScreen(ScreenType.LOAD);
		}

		public void UpdateList()
		{
			list.Refresh();
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.Escape)
				game.ChangeScreen(ScreenType.MENU);
		}
	}
}