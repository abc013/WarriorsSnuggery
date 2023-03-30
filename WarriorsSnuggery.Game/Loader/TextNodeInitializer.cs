using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WarriorsSnuggery.Loader
{
	public class TextNodeInitializer
	{
		readonly List<TextNode> nodes;

		public TextNodeInitializer(List<TextNode> nodes)
		{
			this.nodes = nodes;
		}

		public void SetSaveFields<T>(T @object, bool inherit = true, bool omitDefaults = false)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			var props = typeof(T).GetFields(flags).Cast<MemberInfo>().Concat(typeof(T).GetProperties(flags));
			foreach (var prop in props)
			{
				var attributes = prop.GetCustomAttributes(inherit);
				var saveAttribute = attributes.FirstOrDefault(a => a is SaveAttribute);
				if (saveAttribute == null)
					continue;

				var key = ((SaveAttribute)saveAttribute).Name;
				if (string.IsNullOrEmpty(key))
					key = prop.Name;

				var node = nodes.FirstOrDefault(n => n.Key == key);
				if (node == null)
					continue;

				var type = prop.MemberType == MemberTypes.Property ? typeof(T).GetProperty(prop.Name, flags).PropertyType : typeof(T).GetField(prop.Name, flags).FieldType;
				var value = node.Convert(type);
				
				if (prop.MemberType == MemberTypes.Property)
					typeof(T).GetProperty(prop.Name, flags).SetValue(@object, value);
				else
					typeof(T).GetField(prop.Name, flags).SetValue(@object, value);
			}
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
