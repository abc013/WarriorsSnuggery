namespace WarriorsSnuggery.UI.Objects
{
	class GameSaveList : PanelList
	{
		readonly MPos size;

		public GameSaveList(MPos size, string typeName) : this(size, PanelManager.Get(typeName)) { }

		public GameSaveList(MPos size, PanelType type) : base(size, new MPos(size.X, 1024), type)
		{
			this.size = size;
			foreach (var save in GameSaveManager.Saves)
				if (save.Name != "DEFAULT")
					Add(new GameSaveItem(save, size.X, () => { }));
		}

		public void Refresh()
		{
			Container.Clear();

			foreach (var save in GameSaveManager.Saves)
			{
				if (save.Name != "DEFAULT")
					Add(new GameSaveItem(save, size.X, () => { }));
			}
		}

		public GameSave GetSave()
		{
			for (int i = 0; i < Container.Count; i++)
			{
				var saveItem = (GameSaveItem)Container[i];
				if (saveItem != null && saveItem.Selected)
					return saveItem.Save;
			}

			return null;
		}
	}
}
