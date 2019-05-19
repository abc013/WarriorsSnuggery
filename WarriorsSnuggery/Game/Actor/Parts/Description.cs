using System;

namespace WarriorsSnuggery.Objects.Parts
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
	public class DescAttribute : Attribute
	{
		public readonly string[] Desc;

		public DescAttribute(params string[] desc)
		{
			this.Desc = desc;
		}
	}
}
