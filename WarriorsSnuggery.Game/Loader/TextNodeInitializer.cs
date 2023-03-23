using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Loader
{
	public class TextNodeInitializer
	{
		readonly List<TextNode> nodes;

		public TextNodeInitializer(List<TextNode> nodes)
		{
			this.nodes = nodes;
		}

		public T Convert<T>(string rule, T @default)
		{
			var node = nodes.FirstOrDefault(n => n.Key == rule);
			if (node != null)
				return node.Convert<T>();

			return @default;
		}

        public TextNodeInitializer MakeInitializerWith(string rule)
        {
			var node = nodes.FirstOrDefault(n => n.Key == rule);

            if (node == null)
            {
                var origin = "unknown origin";

                var firstnode = nodes.FirstOrDefault();
                if (firstnode != null)
                    origin = firstnode.Parent.Origin;

				throw new MissingFieldException($"[{origin}] The field '{rule}' was requested but not found.");
            }

            return new TextNodeInitializer(node.Children);
        }
	}
}
