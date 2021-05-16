using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Loader
{
	public class TextNode
	{
		readonly string file;
		public readonly short Order;

		public TextNode Parent;
		public List<TextNode> Children = new List<TextNode>();

		public readonly string Key;
		public readonly string Value;

		public TextNode(string file, short order, string key, object value) : this(file, order, key, value.ToString()) { }

		public TextNode(string file, short order, string key, string value)
		{
			this.file = file;
			Order = order;
			Key = key;
			Value = value;
		}

		public T Convert<T>()
		{
			return Loader.TextNodeConverter.Convert<T>(file, this);
		}

		public object Convert(Type type)
		{
			return Loader.TextNodeConverter.Convert(file, this, type);
		}

		public override string ToString()
		{
			return $"Key '{Key}' | Value '{Value}'";
		}
	}
}
