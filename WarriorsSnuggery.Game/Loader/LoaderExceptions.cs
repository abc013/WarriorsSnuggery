using System;

namespace WarriorsSnuggery.Loader
{
	[Serializable]
	public class InvalidConversionException : Exception
	{
		public InvalidConversionException(TextNode node, Type t)
		  : base($"[{node.Origin}] Unable to convert '{node.Value}' (Key: '{node.Key}') into a value of the type '{t.Name}'."
				+ ((t.IsEnum || t.IsArray && t.GetElementType().IsEnum) ? $" (Valid values are {string.Join(", ", (t.IsArray ? t.GetElementType() : t).GetEnumNames())}.)" : string.Empty))
		{

		}
	}

	[Serializable]
	public class UnexpectedConversionChild : Exception
	{
		public UnexpectedConversionChild(TextNode node, Type t, string child)
		  : base($"[{node.Origin}] Unable to convert '{node.Value}' (Key: '{node.Key}') into a value of the type '{t.Name}' because of the unknown child '{child}'.")
		{
			
		}
	}
}
