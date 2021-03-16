using System;
using System.Runtime.Serialization;

namespace WarriorsSnuggery.Loader
{
	[Serializable]
	public class MissingInfoException : Exception
	{
		public MissingInfoException() { }

		public MissingInfoException(string type) : base($"The type '{type}' does not exist.") { }

		public MissingInfoException(string type, Exception innerException) : base($"The type '{type}' does not exist.", innerException) { }

		protected MissingInfoException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	class MissingNodeException : Exception
	{
		public MissingNodeException() { }

		public MissingNodeException(string rule, string missing) : base($"The rule '{rule}' is missing the required field '{missing}'") { }

		public MissingNodeException(string rule, string missing, Exception innerException) : base($"The rule '{rule}' is missing the required field '{missing}'", innerException) { }

		protected MissingNodeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	class InvalidNodeException : Exception
	{
		public InvalidNodeException() { }

		public InvalidNodeException(string text) : base(text) { }

		public InvalidNodeException(string text, Exception innerException) : base(text, innerException) { }

		protected InvalidNodeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	class InvalidNodeFormatException : Exception
	{
		public InvalidNodeFormatException() { }

		public InvalidNodeFormatException(string rule, Type convertTo) : base($"unable to convert '{rule}' to the type '{convertTo.Name}'.") { }

		public InvalidNodeFormatException(string rule, Type convertTo, Exception innerException) : base($"unable to convert '{rule}' to the type '{convertTo.Name}'.", innerException) { }

		protected InvalidNodeFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	class UnknownNodeException : Exception
	{
		public UnknownNodeException() { }

		public UnknownNodeException(string rule, string parent) : base($"The properties '{rule}' in '{parent}' does not exist.") { }

		public UnknownNodeException(string rule, string parent, Exception innerException) : base($"The properties '{rule}' in '{parent}' does not exist.", innerException) { }

		protected UnknownNodeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	class UnknownPartException : Exception
	{
		public UnknownPartException() { }

		public UnknownPartException(string name) : base($"The part '{name}' does not exist.") { }

		public UnknownPartException(string name, Exception innerException) : base($"The part '{name}' does not exist.", innerException) { }

		protected UnknownPartException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

