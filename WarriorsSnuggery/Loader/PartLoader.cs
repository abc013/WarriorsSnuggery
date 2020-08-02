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
			foreach (var node in nodes)
			{
				var field = fields.FirstOrDefault(f => f.Name == node.Key);
				if (field != null)
				{
					field.SetValue(info, node.Convert(field.FieldType));
					continue;
				}
				throw new YamlUnknownNodeException(node.Key, info.GetType().Name);
			}
		}

		public static PartInfo GetPart(int currentPart, string name, MiniTextNode[] nodes)
		{
			var split = name.Split('@');
			var internalName = currentPart.ToString();
			if (split.Length == 1 || string.IsNullOrWhiteSpace(split[1]))
			{
				name = split[0];
			}
			else
			{
				name = split[0];
				internalName = split[1];
			}

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
