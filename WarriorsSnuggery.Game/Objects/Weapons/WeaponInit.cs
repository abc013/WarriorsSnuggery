using System.Collections.Generic;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class WeaponInit : TextNodeInitializer
	{
		public readonly uint ID;

		public readonly WeaponType Type;

		public WeaponInit(uint id, List<TextNode> nodes, int mapFormat) : base(nodes)
		{
			ID = id;

			Type = Convert<WeaponType>("Type", null);
		}
	}
}
