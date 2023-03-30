using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Loader
{
	public static class TypeLoader
	{
		public static void SetValues(object obj, List<TextNode> nodes, bool checkRequired = true, bool onlyWithSaveAttribute = false)
		{
			var fields = GetFields(obj).Where(f => !onlyWithSaveAttribute || f.GetCustomAttribute<SaveAttribute>() != null).ToList();

			foreach (var node in nodes)
				fields.Remove(SetValue(obj, fields, node, onlyWithSaveAttribute));

			if (!checkRequired || Settings.IgnoreRequiredAttribute)
				return;

			var missing = fields.FirstOrDefault(f => f.GetCustomAttribute<RequireAttribute>() != null);
			if (missing != null)
				throw new MissingFieldException($"The field '{missing.Name}' in '{obj.GetType()}' is required and must be defined!");
		}

		public static IEnumerable<FieldInfo> GetFields(object obj, bool onlyReadonly = true)
		{
			return GetFields(obj.GetType(), onlyReadonly);
		}

		public static IEnumerable<FieldInfo> GetFields(Type type, bool onlyReadonly = true)
		{
			return type.GetFields().Where(f => !onlyReadonly || f.IsInitOnly);
		}

		public static PropertyInfo SetValue(object obj, IEnumerable<PropertyInfo> fields, TextNode node, bool withSaveAttribute = false)
		{
			var field = fields.FirstOrDefault(f => f.Name == node.Key || (withSaveAttribute && f.GetCustomAttribute<SaveAttribute>().Name == node.Key));

			if (field == null)
				throw new UnknownNodeException(node, obj.GetType().Name);

			field.SetValue(obj, node.Convert(field.PropertyType));

			return field;
		}

		public static FieldInfo SetValue(object obj, IEnumerable<FieldInfo> fields, TextNode node, bool withSaveAttribute = false)
		{
			var field = fields.FirstOrDefault(f => f.Name == node.Key || (withSaveAttribute && f.GetCustomAttribute<SaveAttribute>().Name == node.Key));

			if (field == null)
				throw new UnknownNodeException(node, obj.GetType().Name);

			field.SetValue(obj, node.Convert(field.FieldType));

			return field;
		}

		public static PartInfo GetPart(int currentPart, TextNode parent)
		{
			var internalName = currentPart.ToString();

			if (!string.IsNullOrWhiteSpace(parent.Specification))
				internalName = parent.Specification;

			try
			{
				var type = Type.GetType("WarriorsSnuggery.Objects.Actors.Parts." + parent.Key + "PartInfo", true, true);

				var set = new PartInitSet(internalName, parent.Children);

				return (PartInfo)Activator.CreateInstance(type, new [] { set });
			}
			catch (Exception e)
			{
				throw new UnknownPartException(parent, e);
			}
		}
	}
}
