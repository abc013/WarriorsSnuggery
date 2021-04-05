using System;
using System.Runtime.Serialization;

namespace WarriorsSnuggery.Loader
{
	[Serializable]
	public class InvalidConversionException : Exception
	{
		public InvalidConversionException() { }

		public InvalidConversionException(string file, TextNode node, Type t)
		  : base($"[File: {file}] Unable to convert '{node.Value}' (Key: '{node.Key}') into a value of the type '{t.Name}'.")
		{

		}

		public InvalidConversionException(string file, TextNode node, Type t, Exception innerException)
		  : base($"[File: {file}] Unable to convert '{node.Value}' (Key: '{node.Key}') into a value of the type '{t.Name}'.")
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

		public InvalidEnumConversionException(string file, TextNode node, Type t)
		  : base($"[File: {file}] Unable to convert '{node.Value}' (Key: '{node.Key}') into a value of the enum '{t.Name}' (available fields are: {t.GetEnumNames()}).")
		{

		}

		public InvalidEnumConversionException(string file, TextNode node, Type t, Exception innerException)
		  : base($"[File: {file}] Unable to convert '{node.Value}' (Key: '{node.Key}') into a value of the enum '{t.Name}' (available fields are: {t.GetEnumNames()}).", innerException)
		{

		}

		protected InvalidEnumConversionException(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}


	[Serializable]
	public class UnexpectedConversionChild : Exception
	{
		public UnexpectedConversionChild() { }

		public UnexpectedConversionChild(string file, TextNode node, Type t, string child)
		  : base($"[File: {file}] Unable to convert '{node.Value}' (Key: '{node.Key}') into a value of the type '{t.Name}' because of the unknown child '{child}'.")
		{

		}

		public UnexpectedConversionChild(string file, TextNode node, Type t, string child, Exception innerException)
		  : base($"[File: {file}] Unable to convert '{node.Value}' (Key: '{node.Key}') into a value of the type '{t.Name}' because of the unknown child '{child}'.", innerException)
		{

		}

		protected UnexpectedConversionChild(SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{

		}
	}
}
