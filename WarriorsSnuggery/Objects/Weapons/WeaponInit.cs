using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class WeaponInit
	{
		public readonly string File;

		public readonly uint ID;

		public readonly List<MiniTextNode> Nodes;
		public readonly WeaponType Type;

		public readonly CPos Position;
		public readonly int Height;

		public readonly uint OriginID;
		public readonly uint TargetID;

		public WeaponInit(string file, uint id, List<MiniTextNode> nodes)
		{
			File = file;
			ID = id;
			Nodes = nodes;

			Type = Convert<WeaponType>("Type", null);
			Position = Convert("Position", CPos.Zero);
			Height = Convert("Height", 0);

			OriginID = Convert("Origin", uint.MaxValue);
			TargetID = Convert("TargetActor", uint.MaxValue);
		}

		public T Convert<T>(string rule, T @default)
		{
			var node = Nodes.FirstOrDefault(n => n.Key == rule);
			if (node != null)
				return NodeConverter.Convert<T>(File, node);

			return @default;
		}
	}
}
