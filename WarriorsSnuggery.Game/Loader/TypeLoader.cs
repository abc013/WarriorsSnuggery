using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WarriorsSnuggery.Objects.Actors.Parts;

namespace WarriorsSnuggery.Loader
{
	public static class TypeLoader
	{
		public static void SetValues(object obj, List<TextNode> nodes, bool checkRequired = true)
		{
			var changedFields = new List<FieldInfo>();
			var fields = GetFields(obj);

			foreach (var node in nodes)
				changedFields.Add(SetValue(obj, fields, node));

			if (!checkRequired || Settings.IgnoreRequiredAttribute)
				return;

			var requiredFields = fields.Where(f => f.GetCustomAttribute<RequireAttribute>() != null);
			foreach (var required in requiredFields)
            {
				if (!changedFields.Contains(required))
					throw new MissingFieldException($"The field '{required.Name}' in '{obj.GetType()}' is required and must be defined!");
            }
		}

		public static IEnumerable<FieldInfo> GetFields(object obj, bool onlyReadonly = true)
		{
			return GetFields(obj.GetType(), onlyReadonly);
		}

		public static IEnumerable<FieldInfo> GetFields(Type type, bool onlyReadonly = true)
		{
			return type.GetFields().Where(f => !onlyReadonly || f.IsInitOnly);
		}

		public static PropertyInfo SetValue(object obj, IEnumerable<PropertyInfo> fields, TextNode node)
		{
			var field = fields.FirstOrDefault(f => f.Name == node.Key);

			if (field == null)
				throw new UnknownNodeException(node.Key, obj.GetType().Name);

			field.SetValue(obj, node.Convert(field.PropertyType));

			return field;
		}

		public static FieldInfo SetValue(object obj, IEnumerable<FieldInfo> fields, TextNode node)
		{
			var field = fields.FirstOrDefault(f => f.Name == node.Key);

			if (field == null)
				throw new UnknownNodeException(node.Key, obj.GetType().Name);

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
				throw new UnknownPartException(parent.Key, e);
			}
		}
	}
}
