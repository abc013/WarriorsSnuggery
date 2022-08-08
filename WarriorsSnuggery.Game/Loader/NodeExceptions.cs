using System;

namespace WarriorsSnuggery.Loader
{
	[Serializable]
	class InvalidNodeException : Exception
	{
		public InvalidNodeException(string text) : base(text) { }
	}

	[Serializable]
	class UnknownNodeException : Exception
	{
		public UnknownNodeException(TextNode node, string objectName)
			: base($"[{node.Origin}] There is no properties named '{node.Key}' in '{objectName}'.") { }
	}

	[Serializable]
	class UnknownPartException : Exception
	{
		public UnknownPartException(TextNode node)
			: base($"[{node.Origin}] The part '{node.Key}' does not exist.") { }

		public UnknownPartException(TextNode node, Exception innerException)
			: base($"[{node.Origin}] The part '{node.Key}' does not exist.", innerException) { }
	}
}

