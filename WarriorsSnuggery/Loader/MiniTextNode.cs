using System;
using System.Collections.Generic;

namespace WarriorsSnuggery
{
	public class MiniTextNode
	{
		readonly string file;
		public readonly short Order;

		public MiniTextNode Parent;
		public List<MiniTextNode> Children = new List<MiniTextNode>();

		public readonly string Key;
		public readonly string Value;

		public MiniTextNode(string file, short order, string key, string value)
		{
			this.file = file;
			Order = order;
			Key = key;
			Value = value;
		}

		public T Convert<T>()
		{
			return Loader.NodeConverter.Convert<T>(file, this);
		}

		public object Convert(Type type)
		{
			return Loader.NodeConverter.Convert(file, this, type);
		}

		public override string ToString()
		{
			return string.Format("Key '{0}' | Value '{1}'", Key, Value);
		}
	}
}
