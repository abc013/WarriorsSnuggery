namespace WarriorsSnuggery.UI.Objects
{
	public static class PanelCache
	{
		public static readonly TypeDictionary<PanelType> Types = new TypeDictionary<PanelType>();

		public static void Add(PanelType info, string name)
		{
			Types.Add(name, info);
		}
	}
}
