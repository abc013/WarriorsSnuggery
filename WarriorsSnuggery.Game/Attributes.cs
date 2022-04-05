using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WarriorsSnuggery
{
	[AttributeUsage(AttributeTargets.Field)]
	public class RequireAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
	public class DescAttribute : Attribute
	{
		public readonly string[] Desc;

		public DescAttribute(params string[] desc)
		{
			Desc = desc;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class SaveAttribute : Attribute
	{
		public readonly string Name;

		public SaveAttribute()
		{
			Name = string.Empty;
		}

		public SaveAttribute(string name)
		{
			Name = name;
		}

		public static List<string> GetFields<T>(T @object, bool inherit = true, bool omitDefaults = false)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			var list = new List<string>();

			var props = typeof(T).GetProperties(flags).Cast<MemberInfo>().Concat(typeof(T).GetFields(flags));
			foreach (var prop in props)
			{
				var attributes = prop.GetCustomAttributes(inherit);
				var saveAttribute = attributes.FirstOrDefault(a => a is SaveAttribute);
				if (saveAttribute != null)
				{
					var key = ((SaveAttribute)saveAttribute).Name;
					if (string.IsNullOrEmpty(key))
						key = prop.Name;

					var value = prop.MemberType == MemberTypes.Property ? typeof(T).GetProperty(prop.Name, flags).GetValue(@object) : typeof(T).GetField(prop.Name, flags).GetValue(@object);

					if (!omitDefaults)
					{
						var defaultValue = attributes.FirstOrDefault(a => a is DefaultValueAttribute);

						if (defaultValue != null)
						{
							if (value == null && ((DefaultValueAttribute)defaultValue).Default == null)
								continue;

							if (value != null && value.Equals(((DefaultValueAttribute)defaultValue).Default))
								continue;
						}
					}

					list.Add($"{key}={value}");
				}
			}

			return list;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class DefaultValueAttribute : Attribute
	{
		public readonly object Default;

		public DefaultValueAttribute(object @default)
		{
			Default = @default;
		}
	}
}
