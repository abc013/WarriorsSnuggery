using System;
using System.Runtime.Serialization;

namespace WarriorsSnuggery.Maps
{
	[Serializable]
	public class InvalidPieceException : Exception
	{
		public InvalidPieceException() { }

		public InvalidPieceException(string reason) : base(reason) { }

		public InvalidPieceException(string reason, Exception innerException) : base(reason, innerException) { }

		protected InvalidPieceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class MissingPieceException : Exception
	{
		public MissingPieceException() { }

		public MissingPieceException(string piece) : base($"The piece '{piece}' does not exist.") { }

		public MissingPieceException(string piece, Exception innerException) : base($"The piece '{piece}' does not exist.", innerException) { }

		protected MissingPieceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
