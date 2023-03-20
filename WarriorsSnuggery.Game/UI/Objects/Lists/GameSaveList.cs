namespace WarriorsSnuggery.UI.Objects
{
	class GameSaveList : PanelList
	{
		public const int SaveWidth = 4096;

		public GameSave SelectedSave => Selected == null ? null : ((GameSaveItem)Selected).Save;

		public GameSaveList(int height, string typeName) : this(height, PanelCache.Types[typeName]) { }

		public GameSaveList(int height, PanelType type) : base(new UIPos(SaveWidth, height), new UIPos(SaveWidth, 1024), type)
		{
			Refresh();
		}

		public void Refresh()
		{
			Container.Clear();

			foreach (var save in GameSaveManager.Saves)
			{
				if (save.Name != GameSaveManager.DefaultSaveName && !string.IsNullOrEmpty(save.Name))
					Add(new GameSaveItem(save, SaveWidth, () => { }));
			}
		}
	}
}
