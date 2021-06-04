namespace WarriorsSnuggery.UI.Objects
{
	class GameSaveList : PanelList
	{
		public const int SaveWidth = 4096;

		public GameSave SelectedSave => Selected == null ? null : ((GameSaveItem)Selected).Save;

		public GameSaveList(int height, string typeName) : this(height, PanelManager.Get(typeName)) { }

		public GameSaveList(int height, PanelType type) : base(new MPos(SaveWidth, height), new MPos(SaveWidth, 1024), type)
		{
			Refresh();
		}

		public void Refresh()
		{
			Container.Clear();

			foreach (var save in GameSaveManager.Saves)
			{
				if (save.Name != GameSaveManager.DefaultSaveName)
					Add(new GameSaveItem(save, SaveWidth, () => { }));
			}
		}
	}
}
