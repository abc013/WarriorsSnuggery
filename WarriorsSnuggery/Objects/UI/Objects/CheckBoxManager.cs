using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.UI
{
	public static class CheckBoxCreator
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

		public static CheckBox Create(string name, CPos position, bool ticked = false, Action<bool> onTicked = null)
		{
			return new CheckBox(position, GetType(name), ticked, onTicked);
		}
	}
}
