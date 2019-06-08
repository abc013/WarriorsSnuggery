using System;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Loader
{
	static class PartLoader
	{
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
