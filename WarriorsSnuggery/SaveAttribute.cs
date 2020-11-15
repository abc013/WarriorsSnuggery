using System;

namespace WarriorsSnuggery
{
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
}
