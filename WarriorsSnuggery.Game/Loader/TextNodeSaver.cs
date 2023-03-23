using System.Collections.Generic;

namespace WarriorsSnuggery.Loader
{
	public class TextNodeSaver
	{
		readonly List<string> content = new List<string>();

		public TextNodeSaver() { }

		public void GetSaveFields(object obj, bool inherit = true, bool omitDefaults = false)
		{
			content.AddRange(SaveAttribute.GetFields(obj, inherit, omitDefaults));
		}

		public void Add(string name, object value, object defaultValue)
		{
            Add(name, null, value, defaultValue);
		}

		public void Add(string name, string specification, object value, object defaultValue)
		{
			if (value.Equals(defaultValue))
				return;

            Add(name, specification, value);
		}

		public void Add(string name, object value)
		{
			Add(name, null, value);
		}

		public void Add(string name, string specification, object value)
		{
			content.Add(generateContentString(name, specification, value));
		}

		public void AddChildren(string parentName, string specification, TextNodeSaver children)
		{
            Add(parentName, string.Empty);
            foreach (var contentString in children.content)
                content.Add($"\t{contentString}");
		}

        string generateContentString(string name, string specification, object value)
        {
            return $"{name}{(string.IsNullOrEmpty(specification) ? string.Empty : $"@{specification}")}={value}";
        }
	}
}
