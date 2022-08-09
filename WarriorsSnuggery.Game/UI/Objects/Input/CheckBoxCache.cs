namespace WarriorsSnuggery.UI.Objects
{
	public static class CheckBoxCache
	{
		public static readonly TypeDictionary<CheckBoxType> Types = new TypeDictionary<CheckBoxType>();

		public static void Add(CheckBoxType info, string name)
		{
			Types.Add(name, info);
		}
	}
}
