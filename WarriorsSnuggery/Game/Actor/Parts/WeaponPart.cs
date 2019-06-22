namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Adds a weapon to the object.")]
	public class WeaponPartInfo : PartInfo
	{
		[Desc("Name of the weapon.")]
		public readonly WeaponType Type;
		[Desc("Offset of the shoot point relative to the object's center.")]
		public readonly CPos Offset;
		[Desc("Height of the shoot point.")]
		public readonly int Height;

		public override ActorPart Create(Actor self)
		{
			return new WeaponPart(self, this);
		}

		public WeaponPartInfo(MiniTextNode[] nodes) : base(nodes)
		{

		}
	}
	
	public class WeaponPart : ActorPart
	{
		readonly WeaponPartInfo info;
		public readonly WeaponType Type;

		public CPos WeaponOffsetPosition
		{
			get { return self.GraphicPositionWithoutHeight + info.Offset; }
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
			weapon.Height = info.Height;
			self.World.Add(weapon);

			return weapon;
		}
	}
}
