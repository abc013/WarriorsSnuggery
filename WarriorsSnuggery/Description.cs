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
}
