namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Adds a weapon to the object.")]
	public class WeaponPartInfo : PartInfo
	{
		[Desc("Name of the weapon.")]
		public readonly WeaponType Type;
		[Desc("Offset of the shoot point relative to the object's center.")]
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
						Type = WeaponCreator.GetType(node.Value);
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
	
	public class WeaponPart : ActorPart
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
			Type = info.Type;
		}

		public Weapon OnAttack(CPos target)
		{
			var weapon = WeaponCreator.Create(self.World, info.Type, self, target);
			//weapon.Height = Height; // TODO
			self.World.Add(weapon);

			return weapon;
		}
	}
}
