using System;

namespace WarriorsSnuggery.Objects.Parts
{
	public class WeaponPartInfo : PartInfo
	{
		public readonly WeaponType WeaponType;
		public readonly CPos Offset;

		public override ActorPart Create(Actor self)
		{
			return new WeaponPart(self, this);
		}

		public WeaponPartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Type":
						WeaponType = WeaponCreator.GetType(node.Value);
						break;
					case "Offset":
						Offset = node.ToCPos();
						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "WeaponPart");
				}
			}
		}
	}

	/// <summary>
	/// Adds a Weapon to the Actor.
	/// </summary>
	public class WeaponPart : ActorPart // TODO unused
	{
		readonly WeaponPartInfo info;
		public readonly WeaponType Type;

		public CPos WeaponOffsetPosition
		{
			get { return self.GraphicPosition + info.Offset; }
			set { }
		}

		public WeaponPart(Actor self, WeaponPartInfo info) : base(self)
		{
			this.info = info;
			Type = info.WeaponType;
		}

		public Weapon OnAttack(CPos target)
		{
			var weapon = WeaponCreator.Create(self.World, info.WeaponType, self, target);
			//weapon.Height = Height; // TODO
			self.World.Add(weapon);

			return weapon;
		}
	}
}
