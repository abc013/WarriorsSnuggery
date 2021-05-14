using System;

namespace WarriorsSnuggery
{
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
