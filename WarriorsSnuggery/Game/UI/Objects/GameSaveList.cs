using System;

namespace WarriorsSnuggery.UI
{
	class GameSaveList : PanelList
	{
		readonly string saveTexture;
		readonly MPos size;
		public GameSaveList(CPos pos, MPos size, int bordersize, string texture, string saveTexture, string borderTexture = "", string activeTexture = "") : base(pos, size, new MPos(size.X, 1024), bordersize, texture, borderTexture, activeTexture)
		{
			this.size = size;
			this.saveTexture = saveTexture;
			foreach (var statistic in GameSaveManager.Statistics)
				if (statistic.Name != "DEFAULT")
					Add(new GameSaveItem(CPos.Zero, statistic, saveTexture, size.X, () => { }));
		}

		public void Refresh()
		{
			foreach (var o in Container)
				o.Dispose();
			Container.Clear();

			foreach (var statistic in GameSaveManager.Statistics)
			{
				if (statistic.Name != "DEFAULT")
					Add(new GameSaveItem(CPos.Zero, statistic, saveTexture, size.X, () => { }));
			}
		}

		public SaveStatistics GetStatistic()
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
