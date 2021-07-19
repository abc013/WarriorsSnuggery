namespace WarriorsSnuggery.UI.Objects
{
	public static class CheckBoxManager
	{
		public static readonly TypeDictionary<CheckBoxType> Types = new TypeDictionary<CheckBoxType>();

		public static void AddType(CheckBoxType info, string name)
		{
			Types.Add(name, info);
		}
	}
}
