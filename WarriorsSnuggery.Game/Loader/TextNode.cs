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

		public readonly string Origin;

		public TextNode(string origin, short order, string key, object value) : this(origin, order, key, null, value) { }

		public TextNode(string origin, short order, string key, string specification, object value)
		{
			Order = order;
			Key = key;
			Specification = specification;
			Value = value.ToString();

			Origin = origin;
		}

		public T Convert<T>()
		{
			return TextNodeConverter.Convert<T>(this);
		}

		public object Convert(Type type)
		{
			return TextNodeConverter.Convert(this, type);
		}

		public override string ToString()
		{
			var @string = ToIdentifierString();

			if (!string.IsNullOrEmpty(Value))
				@string += $"={Value}";

			return @string;
		}

		public string ToIdentifierString()
		{
			var @string = $"{Key}";

			if (!string.IsNullOrEmpty(Specification))
				@string += $"@{Specification}";

			return @string;
		}
	}
}
