using System;
using System.Runtime.Serialization;

namespace WarriorsSnuggery.Loader
{
	[Serializable]
	public class InvalidConversionException : Exception
	{
		public InvalidConversionException() { }

		public InvalidConversionException(string file, MiniTextNode node, Type t)
		  : base(string.Format(@"[File: {0}] Unable to convert '{1}' (Key: '{2}') into a value of the type '{3}'.", file, node.Value, node.Key, t))
		{

		}

		public InvalidConversionException(string file, MiniTextNode node, Type t, Exception innerException)
		  : base(string.Format(@"[File: {0}] Unable to convert '{1}' (Key: '{2}') into a value of the type '{3}'.", file, node.Value, node.Key, t), innerException)
		{

		}

		protected InvalidConversionException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}

	[Serializable]
	public class InvalidEnumConversionException : Exception
	{
		public InvalidEnumConversionException() { }

		public InvalidEnumConversionException(string file, MiniTextNode node, Type t)
		  : base(string.Format(@"[File: {0}] Unable to convert '{1}' (Key: '{2}') into a value of the enum '{3}' (available fields are: {4}).", file, node.Value, node.Key, t, t.GetEnumNames()))
		{

		}

		public InvalidEnumConversionException(string file, MiniTextNode node, Type t, Exception innerException)
		  : base(string.Format(@"[File: {0}] Unable to convert '{1}' (Key: '{2}') into a value of the enum '{3}' (available fields are: {4}).", file, node.Value, node.Key, t, t.GetEnumNames()), innerException)
		{

		}

		protected InvalidEnumConversionException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}
}
