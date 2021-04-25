using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Loader
{
	public static class TypeLoader
	{
		public static void SetValues(object obj, List<TextNode> nodes)
		{
			var fields = GetFields(obj);

			foreach (var node in nodes)
				SetValue(obj, fields, node);
		}

		public static IEnumerable<FieldInfo> GetFields(object obj, bool onlyReadonly = true)
		{
			return GetFields(obj.GetType(), onlyReadonly);
		}

		public static IEnumerable<FieldInfo> GetFields(Type type, bool onlyReadonly = true)
		{
			return type.GetFields().Where(f => !onlyReadonly || f.IsInitOnly);
		}

		public static void SetValue(object obj, IEnumerable<PropertyInfo> fields, TextNode node)
		{
			var field = fields.FirstOrDefault(f => f.Name == node.Key);

			if (field == null)
				throw new UnknownNodeException(node.Key, obj.GetType().Name);

			field.SetValue(obj, node.Convert(field.PropertyType));
		}

		public static void SetValue(object obj, IEnumerable<FieldInfo> fields, TextNode node)
		{
			var field = fields.FirstOrDefault(f => f.Name == node.Key);

			if (field == null)
				throw new UnknownNodeException(node.Key, obj.GetType().Name);

			field.SetValue(obj, node.Convert(field.FieldType));
		}

		public static PartInfo GetPart(int currentPart, string name, List<TextNode> nodes)
		{
			var split = name.Split('@');
			var internalName = currentPart.ToString();

			name = split[0];
			if (split.Length > 1 && !string.IsNullOrWhiteSpace(split[1]))
				internalName = split[1];

			try
			{
				var type = Type.GetType("WarriorsSnuggery.Objects.Parts." + name + "PartInfo", true, true);

				var set = new PartInitSet(internalName, nodes);

				return (PartInfo)Activator.CreateInstance(type, new [] { set });
			}
			catch (Exception e)
			{
				throw new UnknownPartException(name, e);
			}
		}
	}
}
