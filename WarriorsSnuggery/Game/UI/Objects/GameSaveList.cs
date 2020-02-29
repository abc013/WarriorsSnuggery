

namespace WarriorsSnuggery.UI
{
	class GameSaveList : PanelList
	{
		readonly MPos size;
		public GameSaveList(CPos pos, MPos size, PanelType type) : base(pos, size, new MPos(size.X, 1024), type)
		{
			this.size = size;
			foreach (var statistic in GameSaveManager.Statistics)
				if (statistic.Name != "DEFAULT")
					Add(new GameSaveItem(CPos.Zero, statistic, size.X, () => { }));
		}

		public void Refresh()
		{
			Container.Clear();

			foreach (var statistic in GameSaveManager.Statistics)
			{
				if (statistic.Name != "DEFAULT")
					Add(new GameSaveItem(CPos.Zero, statistic, size.X, () => { }));
			}
		}

		public GameStatistics GetStatistic()
		{
			for (int i = 0; i < Container.Count; i++)
			{
				GameSaveItem stats = (GameSaveItem)Container[i];
				if (stats != null && stats.Selected)
					return stats.Stats;
			}

			return null;
		}
	}
}
