using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public static class ButtonCreator
	{
		static readonly Dictionary<string, ButtonType> types = new Dictionary<string, ButtonType>();

		public static void AddType(ButtonType info, string name)
		{
			types.Add(name, info);
		}

		public static ButtonType GetType(string name)
		{
			if (!types.ContainsKey(name))
				throw new MissingInfoException(name);

			return types[name];
		}

		public static Button Create(string name, CPos position, string text, Action func)
		{
			return new Button(position, text, GetType(name), func);
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
		static readonly Dictionary<string, TextBoxType> types = new Dictionary<string, TextBoxType>();

		public static void AddType(TextBoxType info, string name)
		{
			types.Add(name, info);
		}

		public static TextBoxType GetType(string name)
		{
			if (!types.ContainsKey(name))
				throw new MissingInfoException(name);

			return types[name];
		}

		public static TextBox Create(string name, CPos position, string text, int maximumLength = 10, bool onlyNumbers = false, Action onEnter = null)
		{
			return new TextBox(position, text, maximumLength, onlyNumbers, GetType(name), onEnter);
		}
	}
}
