using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Loader
{
	public class TextNode
	{
		public TextNode Parent;
		public List<TextNode> Children = new List<TextNode>();

		public readonly short Order;

		public readonly string Key;
		public readonly string Specification;
		public readonly string Value;

		readonly string file;

		public TextNode(string file, short order, string key, object value) : this(file, order, key, null, value) { }

		public TextNode(string file, short order, string key, string specification, object value)
		{
			Order = order;
			Key = key;
			Specification = specification;
			Value = value.ToString();

			this.file = file;
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
			if (Specification != null)
				return $"Key '{Key}', Spec '{Specification}' | Value '{Value}'";

			return $"Key '{Key}' | Value '{Value}'";
		}
	}
}
