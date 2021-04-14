using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.UI
{
	public static class CheckBoxManager
	{
		static readonly Dictionary<string, CheckBoxType> types = new Dictionary<string, CheckBoxType>();

		public static void AddType(CheckBoxType info, string name)
		{
			types.Add(name, info);
		}

		public static CheckBoxType GetType(string name)
		{
			if (!types.ContainsKey(name))
				throw new MissingInfoException(name);

			return types[name];
		}
	}
}
