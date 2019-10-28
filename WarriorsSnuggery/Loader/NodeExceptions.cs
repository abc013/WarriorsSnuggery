/*
 * User: Andreas
 * Date: 29.07.2018
 * Time: 17:18
 */
using System;
using System.Runtime.Serialization;

namespace WarriorsSnuggery
{
	[Serializable]
	public class InvalidPieceException : Exception
	{
		public InvalidPieceException() { }

		public InvalidPieceException(string text)
		  : base(text)
		{

		}

		public InvalidPieceException(string text, Exception innerException)
			: base(text, innerException)
		{

		}

		protected InvalidPieceException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}

	[Serializable]
	public class MissingPieceException : Exception
	{
		public MissingPieceException() { }

		public MissingPieceException(string piece)
		  : base(string.Format(@"The piece '{0}' does not exist.", piece))
		{

		}

		public MissingPieceException(string piece, Exception innerException)
			: base(string.Format(@"The piece '{0}' does not exist.", piece), innerException)
		{

		}

		protected MissingPieceException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}

	[Serializable]
	public class MissingInfoException : Exception
	{
		public MissingInfoException() { }

		public MissingInfoException(string type)
		  : base(string.Format(@"The type '{0}' does not exist.", type))
		{

		}

		public MissingInfoException(string type, Exception innerException)
			: base(string.Format(@"The type '{0}' does not exist.", type), innerException)
		{

		}

		protected MissingInfoException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}

	[Serializable]
	class YamlMissingNodeException : Exception
	{
		public YamlMissingNodeException() { }

		public YamlMissingNodeException(string rule, string missing)
		  : base(string.Format(@"The rule '{0}' is missing the required field '{1}'", rule, missing))
		{

		}

		public YamlMissingNodeException(string rule, string missing, Exception innerException)
			: base(string.Format(@"The rule '{0}' is missing the required field '{1}'", rule, missing), innerException)
		{

		}

		protected YamlMissingNodeException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}

	[Serializable]
	class YamlInvalidNodeException : Exception
	{
		public YamlInvalidNodeException() { }

		public YamlInvalidNodeException(string text)
		  : base(text)
		{

		}

		public YamlInvalidNodeException(string text, Exception innerException)
			: base(text, innerException)
		{

		}

		protected YamlInvalidNodeException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}

	[Serializable]
	class YamlInvalidFormatException : Exception
	{
		public YamlInvalidFormatException() { }

		public YamlInvalidFormatException(string rule, Type convertTo)
		  : base(string.Format(@"unable to convert '{0}' to the type '{1}' .", rule, convertTo.Name))
		{

		}

		public YamlInvalidFormatException(string rule, Type convertTo, Exception innerException)
			: base(string.Format(@"unable to convert '{0}' to the type '{1}' .", rule, convertTo.Name), innerException)
		{

		}

		protected YamlInvalidFormatException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}

	[Serializable]
	class YamlInvalidRuleExeption : Exception
	{
		public YamlInvalidRuleExeption() { }

		public YamlInvalidRuleExeption(string rule)
		  : base(string.Format(@"unable to convert '{0}' to a valid rule.", rule))
		{

		}

		public YamlInvalidRuleExeption(string rule, Exception innerException)
		  : base(string.Format(@"unable to convert '{0}' to a valid rule.", rule), innerException)
		{

		}

		public YamlInvalidRuleExeption(string rule, int tabs)
		  : base(string.Format(@"'{0}' has wrong spacing (difference: {1}).", rule, tabs))
		{

		}

		public YamlInvalidRuleExeption(string rule, int tabs, Exception innerException)
		  : base(string.Format(@"'{0}' has wrong spacing (difference: {1}).", rule, tabs), innerException)
		{

		}

		protected YamlInvalidRuleExeption(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}

	[Serializable]
	class YamlUnknownNodeException : Exception
	{
		public YamlUnknownNodeException() { }

		public YamlUnknownNodeException(string rule, string parent)
			: base(string.Format(@"The properties '{0}' in '{1}' does not exist.", rule, parent))
		{

		}

		public YamlUnknownNodeException(string rule, string parent, Exception innerException)
			: base(string.Format(@"The properties '{0}' in '{1}' does not exist.", rule, parent), innerException)
		{

		}

		protected YamlUnknownNodeException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}

	[Serializable]
	class YamlUnknownPartException : Exception
	{
		public YamlUnknownPartException() { }

		public YamlUnknownPartException(string name)
			: base(string.Format(@"The part '{0}' does not exist.", name))
		{

		}

		public YamlUnknownPartException(string name, Exception innerException)
			: base(string.Format(@"The part '{0}' does not exist.", name), innerException)
		{

		}

		protected YamlUnknownPartException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}
}

