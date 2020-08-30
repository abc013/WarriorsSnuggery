using System.Collections.Generic;

namespace WarriorsSnuggery.UI
{
	public static class PanelManager
	{
		static readonly Dictionary<string, PanelType> types = new Dictionary<string, PanelType>();

		public static void AddType(PanelType info, string name)
		{
			types.Add(name, info);
		}

		public static PanelType Get(string name)
		{
			if (!types.ContainsKey(name))
				throw new MissingInfoException(name);

			return types[name];
		}
	}
}
