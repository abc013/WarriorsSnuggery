using System;
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

	public static class ButtonCreator
	{
		public static Button Create(string type, CPos position, string text, Action onClick)
		{
			return new Button(position, text, PanelManager.Get(type), onClick);
		}
	}

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

		public static CheckBox Create(string name, CPos position, bool ticked, Action<bool> onTicked = null)
		{
			return new CheckBox(position, ticked, GetType(name), onTicked);
		}
	}

	public static class TextBoxCreator
	{
		public static TextBox Create(string type, CPos position, string text, int maximumLength = 10, bool onlyNumbers = false, bool isPath = false)
		{
			return new TextBox(position, text, maximumLength, onlyNumbers, isPath, PanelManager.Get(type));
		}
	}
}
