using System;
using System.Linq;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Loader
{
	public static class PartLoader
	{
		public static void SetValues(object info, MiniTextNode[] nodes)
		{
			var fields = info.GetType().GetFields().Where(f => f.IsInitOnly);
			foreach(var node in nodes)
			{
				var field = fields.FirstOrDefault(f => f.Name == node.Key);
				if (field != null)
				{
					field.SetValue(info, node.Convert(field.FieldType));
					continue;
				}
				throw new YamlUnknownNodeException(node.Key, info.GetType().Name);
			}
			// other variant
			//foreach (var attrib in info.GetType().GetFields().Where(f => f.IsInitOnly))
			//{
			//	var type = attrib.FieldType;
			//	var node = nodes.FirstOrDefault(n => n.Key == attrib.Name);

			//	if (node != null)
			//		attrib.SetValue(info, node.Convert(type));
			//}
		}

		public static bool IsPart(string name)
		{
			return name.Contains("@");
		}

		public static PartInfo GetPart(string name, MiniTextNode[] nodes)
		{
			if (!IsPart(name))
				return null;

			name = name.Remove(0, name.IndexOf('@') + 1);

			try
			{
				var type = Type.GetType("WarriorsSnuggery.Objects.Parts." + name + "PartInfo", true, true);

				return (PartInfo)Activator.CreateInstance(type, new[] { nodes });
			}
			catch (Exception e)
			{
				throw new YamlUnknownPartException(name, e);
			}
		}
	}
}
