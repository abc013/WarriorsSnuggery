namespace WarriorsSnuggery.UI.Objects
{
	class ModList : PanelList
	{
		public const int ModWidth = 3072;
		public const int ModHeight = 512;
		readonly bool useActive;

		public Mod SelectedMod => Selected == null ? null : ((ModItem)Selected).Mod;

		public ModList(int height, string typeName, bool useActive) : this(height, PanelCache.Types[typeName], useActive) { }

		public ModList(int height, PanelType type, bool useActive) : base(new MPos(ModWidth, height), new MPos(ModWidth, ModHeight), type)
		{
			this.useActive = useActive;

			Refresh();
		}

		public void Refresh()
		{
			Container.Clear();
			SelectedPos = (-1, -1);

			foreach (var mod in ModManager.AvailableMods)
			{
				var modActive = Settings.ModList.Contains(mod.InternalName);
				if (!(useActive ^ modActive))
					Add(new ModItem(mod, ModWidth, () => { }));
			}
		}
	}
}
