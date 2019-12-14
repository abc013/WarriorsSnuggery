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
		public int WeaponHeightPosition
		{
			get { return self.Height + info.Height; }
			set { }
		}

		public CPos Target;
		BeamWeapon beam;

		public WeaponPart(Actor self, WeaponPartInfo info) : base(self)
		{
			this.info = info;
			Type = info.Type;
		}

		public Weapon OnAttack(CPos target)
		{
			Target = target;
			var weapon = WeaponCreator.Create(self.World, info.Type, self, target);
			if (weapon is BeamWeapon)
			{
				beam = (BeamWeapon)weapon;
				beam.OriginHeight = WeaponHeightPosition;
			}
			else
				weapon.Height = WeaponHeightPosition;

			self.World.Add(weapon);

			return weapon;
		}

		public override void Tick()
		{
			if (beam != null)
			{
				if (beam.Disposed)
				{
					beam = null;
					return;
				}
				beam.Target = Target;
			}
		}

		public override void OnDispose()
		{
			if (beam != null && !beam.Disposed)
				beam.Dispose();
		}
	}
}
