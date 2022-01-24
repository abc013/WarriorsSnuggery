using System;
using System.Collections.Generic;
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

		public static List<string> GetFields<T>(T @object, bool inherit = true)
		{
			var list = new List<string>();

			var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var prop in props)
			{
				var attributes = prop.GetCustomAttributes(inherit);
				foreach (var attribute in attributes)
				{
					if (attribute is not SaveAttribute saveAttribute)
						continue;

					var key = string.IsNullOrEmpty(saveAttribute.Name) ? prop.Name : saveAttribute.Name;
					var value = prop.GetValue(@object);
					list.Add($"{key}={value}");
				}
			}
			var varis = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var vari in varis)
			{
				var attributes = vari.GetCustomAttributes(inherit);
				foreach (var attribute in attributes)
				{
					if (attribute is not SaveAttribute saveAttribute)
						continue;

					var key = string.IsNullOrEmpty(saveAttribute.Name) ? vari.Name : saveAttribute.Name;
					var value = vari.GetValue(@object);
					list.Add($"{key}={value}");
				}
			}

			return list;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class DefaultValueAttribute : Attribute
	{
		public readonly object Default;

		public DefaultValueAttribute(object @default)
		{
			Default = @default;
		}
	}
}
