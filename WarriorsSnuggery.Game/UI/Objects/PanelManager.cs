namespace WarriorsSnuggery.UI.Objects
{
	public static class PanelManager
	{
		public static readonly TypeDictionary<PanelType> Types = new TypeDictionary<PanelType>();

		public static void AddType(PanelType info, string name)
		{
			Types.Add(name, info);
		}
	}
}
