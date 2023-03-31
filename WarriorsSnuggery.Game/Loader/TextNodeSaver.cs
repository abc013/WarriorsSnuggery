using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WarriorsSnuggery.Loader
{
	public class TextNodeSaver
	{
		readonly List<string> content = new List<string>();

		public TextNodeSaver() { }

		public void AddSaveFields<T>(T @object, bool inherit = true, bool omitDefaults = false)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			var props = typeof(T).GetProperties(flags).Cast<MemberInfo>().Concat(typeof(T).GetFields(flags));
			foreach (var prop in props)
			{
				var saveAttribute = prop.GetCustomAttribute<SaveAttribute>();
				if (saveAttribute == null)
					continue;

				var key = saveAttribute.Name;
				if (string.IsNullOrEmpty(key))
					key = prop.Name;

				var value = prop.MemberType == MemberTypes.Property ? typeof(T).GetProperty(prop.Name, flags).GetValue(@object) : typeof(T).GetField(prop.Name, flags).GetValue(@object);
				var type = prop.MemberType == MemberTypes.Property ? typeof(T).GetProperty(prop.Name, flags).PropertyType : typeof(T).GetField(prop.Name, flags).FieldType;

				if (!omitDefaults)
				{
					var defaultValue = prop.GetCustomAttribute<DefaultValueAttribute>();

					if (defaultValue != null)
					{
						if (value == null && defaultValue.Default == null)
							continue;

						if (value != null && value.Equals(defaultValue.Default))
							continue;
					}
				}

				if (type.IsArray)
				{
					var enumeration = string.Empty;
					foreach (var intern in (Array)value)
						enumeration += intern + ",";

					content.Add($"{key}={enumeration.TrimEnd(',')}");
				}
				else
					content.Add($"{key}={value}");
			}
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

		public void Append(TextNodeSaver other)
		{
			content.AddRange(other.content);
		}

		public void AddChildren(string parentName, TextNodeSaver children, bool ignoreIfNoChildren = false)
		{
			AddChildren(parentName, string.Empty, children, ignoreIfNoChildren);
		}

		public void AddChildren(string parentName, string specification, TextNodeSaver children, bool ignoreIfNoChildren = false)
		{
			if (children.content.Count == 0 && ignoreIfNoChildren)
				return;

            Add(parentName, specification, string.Empty);
            foreach (var contentString in children.content)
                content.Add($"\t{contentString}");
		}

        string generateContentString(string name, string specification, object value)
        {
            return $"{name}{(string.IsNullOrEmpty(specification) ? string.Empty : $"@{specification}")}={value}";
        }

		public List<string> GetStrings()
		{
			return content;
		}
	}
}
