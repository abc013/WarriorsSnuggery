namespace WarriorsSnuggery.UI.Objects
{
	class PackageList : PanelList
	{
		public const int ItemWidth = 3072;
		public const int ItemHeight = 512;
		readonly bool useActive;

		public Package SelectedPackage => Selected == null ? null : ((PackageItem)Selected).Package;

		public PackageList(int height, string typeName, bool useActive) : this(height, PanelCache.Types[typeName], useActive) { }

		public PackageList(int height, PanelType type, bool useActive) : base(new MPos(ItemWidth, height), new MPos(ItemWidth, ItemHeight), type)
		{
			this.useActive = useActive;

			Refresh();
		}

		public void Refresh()
		{
			Container.Clear();
			SelectedPos = (-1, -1);

			foreach (var package in PackageManager.AvailablePackages)
			{
				var modActive = Settings.PackageList.Contains(package.InternalName);
				if (!(useActive ^ modActive))
					Add(new PackageItem(package, ItemWidth, () => { }));
			}
		}
	}
}
