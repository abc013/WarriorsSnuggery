using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Loader
{
	public static class PartLoader
	{

		public static void SetValues(object obj, MiniTextNode[] nodes)
		{
			var fields = GetFields(obj);

			foreach (var node in nodes)
				SetValue(obj, fields, node);
		}

		public static IEnumerable<FieldInfo> GetFields(object obj, bool onlyReadonly = true)
		{
			return obj.GetType().GetFields().Where(f => !onlyReadonly || f.IsInitOnly);
		}

		public static void SetValue(object obj, IEnumerable<FieldInfo> fields, MiniTextNode node)
		{
			var field = fields.FirstOrDefault(f => f.Name == node.Key);

			if (field == null)
				throw new YamlUnknownNodeException(node.Key, obj.GetType().Name);

			field.SetValue(obj, node.Convert(field.FieldType));
		}

		public static PartInfo GetPart(int currentPart, string name, MiniTextNode[] nodes)
		{
			var split = name.Split('@');
			var internalName = currentPart.ToString();

			name = split[0];
			if (split.Length > 1 && !string.IsNullOrWhiteSpace(split[1]))
				internalName = split[1];

			try
			{
				var type = Type.GetType("WarriorsSnuggery.Objects.Parts." + name + "PartInfo", true, true);

				return (PartInfo)Activator.CreateInstance(type, new object[] { internalName, nodes });
			}
			catch (Exception e)
			{
				throw new YamlUnknownPartException(name, e);
			}
		}
	}
}
