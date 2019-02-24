/*
 * User: Andreas
 * Date: 17.09.2018
 * Time: 15:58
 */
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
