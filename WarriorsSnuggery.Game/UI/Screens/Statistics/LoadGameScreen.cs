﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.UI.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	class LoadGameScreen : Screen
	{
		readonly Game game;

		readonly GameSaveList list;

		public LoadGameScreen(Game game) : base("Load Game")
		{
			this.game = game;
			Title.Position = new CPos(0, -4096, 0);

			list = new GameSaveList(new MPos((int)(WindowInfo.UnitWidth * 128), 4096), "wooden") { Position = new CPos(0, 1024, 0) };

			Add(new Button("Back", "wooden", () => game.ShowScreen(ScreenType.MENU)) { Position = new CPos(4096, 6144, 0) });
			void loadAction()
			{
				var stats = list.GetStatistic();
				if (stats != null)
				{
					humanAgreeOnLoad(() =>
					{
						Log.WriteDebug("Loading a game save: " + stats.SaveName);
						GameController.CreateNew(new GameStatistics(stats), loadStatsMap: true);
					}, "Are you sure you want to load this save? Unsaved progress will be lost!");
				}
			}
			Add(new Button("Load", "wooden", loadAction) { Position = new CPos(0, 6144, 0) });

			void deleteAction()
			{
				var stats = list.GetStatistic();
				if (stats != null)
				{
					humanAgreeOnDelete(() =>
					{
						GameSaveManager.Delete(stats);
						game.RefreshSaveGameScreens();
						game.ShowScreen(ScreenType.LOADGAME);
						Log.WriteDebug("Deleting a game save: " + stats.SaveName);
					}, "Are you sure you want to delete this save?");
				}
			}
			Add(new Button("Delete", "wooden", deleteAction) { Position = new CPos(-4096, 6144, 0) });

			Add(list);
		}

		public override void Hide()
		{
			list.DisableTooltip();
		}

		void humanAgreeOnLoad(Action onAgree, string text)
		{
			if (game.MissionType == MissionType.MAIN_MENU)
				onAgree();

			game.ShowDecisionScreen(onDecline, onAgree, text);
		}

		void humanAgreeOnDelete(Action onAgree, string text)
		{
			game.ShowDecisionScreen(onDecline, onAgree, text);
		}

		void onDecline()
		{
			game.ShowScreen(ScreenType.LOADGAME);
		}

		public void UpdateList()
		{
			list.Refresh();
		}

		public override void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			if (key == Keys.Escape)
				game.ShowScreen(ScreenType.MENU);
		}
	}
}
