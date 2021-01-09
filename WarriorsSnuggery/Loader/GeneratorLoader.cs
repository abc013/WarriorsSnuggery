using System;
using System.Collections.Generic;
using WarriorsSnuggery.Maps.Generators;

namespace WarriorsSnuggery.Loader
{
	public static class GeneratorLoader
	{
		public static IMapGeneratorInfo GetGenerator(string name, int id, List<MiniTextNode> nodes)
		{
			try
			{
				var type = Type.GetType("WarriorsSnuggery.Maps.Generators." + name + "Info", true, true);

				return (IMapGeneratorInfo)Activator.CreateInstance(type, new object[] { id, nodes });
			}
			catch (Exception e)
			{
				throw new UnknownGeneratorException(name, e);
			}
		}
	}
}
