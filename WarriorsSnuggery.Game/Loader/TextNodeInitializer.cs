using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WarriorsSnuggery.Loader
{
	public class TextNodeInitializer
	{
		protected readonly List<TextNode> Nodes;

		public TextNodeInitializer(List<TextNode> nodes)
		{
			Nodes = nodes;
		}

		public void SetSaveFields<T>(T @object, bool inherit = true, bool ignoreDefaults = false)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			var props = typeof(T).GetFields(flags).Cast<MemberInfo>().Concat(typeof(T).GetProperties(flags));
			foreach (var prop in props)
			{
				var saveAttribute = prop.GetCustomAttribute<SaveAttribute>();
				if (saveAttribute == null)
					continue;

				var key = saveAttribute.Name;
				if (string.IsNullOrEmpty(key))
					key = prop.Name;

				var type = prop.MemberType == MemberTypes.Property ? typeof(T).GetProperty(prop.Name, flags).PropertyType : typeof(T).GetField(prop.Name, flags).FieldType;

				var node = Nodes.FirstOrDefault(n => n.Key == key);
				object value;
				if (node == null)
				{
					if (ignoreDefaults)
						continue;

					var defaultAttribute = prop.GetCustomAttribute<DefaultValueAttribute>();
					if (defaultAttribute == null)
						continue;

					value = defaultAttribute.Default;
				}
				else
					value = node.Convert(type);
				
				if (prop.MemberType == MemberTypes.Property)
					typeof(T).GetProperty(prop.Name, flags).SetValue(@object, value);
				else
					typeof(T).GetField(prop.Name, flags).SetValue(@object, value);
			}
		}

		public T Convert<T>(string rule, T @default)
		{
			var node = Nodes.FirstOrDefault(n => n.Key == rule);
			if (node != null)
				return node.Convert<T>();

			return @default;
		}

		public bool ContainsRule(string rule)
		{
			return Nodes.Exists(n => n.Key == rule);
		}

        public TextNodeInitializer MakeInitializerWith(string rule)
        {
			var node = Nodes.FirstOrDefault(n => n.Key == rule);

            if (node == null)
            {
                var origin = "unknown origin";

                var firstnode = Nodes.FirstOrDefault();
                if (firstnode != null)
                    origin = firstnode.Parent.Origin;

				throw new MissingFieldException($"[{origin}] The field '{rule}' was requested but not found.");
            }

            return new TextNodeInitializer(node.Children);
        }
	}
}
