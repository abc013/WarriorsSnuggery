using System;

namespace WarriorsSnuggery.Objects
{
	public class TextBoxType
	{
		public readonly string DefaultString;
		public readonly string ActiveString;
		public readonly string BorderString;
		public readonly int Border;

		public TextBoxType(string defaultString, string activeString, string borderString, int border)
		{
			DefaultString = defaultString;
			ActiveString = activeString;
			BorderString = borderString;
			Border = border;
		}
	}
}
