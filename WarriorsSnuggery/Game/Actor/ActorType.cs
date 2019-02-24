using System;

namespace WarriorsSnuggery.Objects
{
	public class ActorType
	{
		public readonly int PhysicalSize;
		public readonly Shape PhysicalShape;

		public readonly TextureInfo Idle;
		public readonly int IdleFacings;
		public readonly TextureInfo Walk;
		public readonly int WalkFacings;
		public readonly TextureInfo Attack;
		public readonly int AttackFacings;
		public readonly TextureInfo Shadow;

		public readonly CPos Offset;
		public readonly float Scale;
		public readonly int Height;

		public readonly int Mana;
		public readonly WeaponType ManaWeapon;
		public readonly int ManaCost;
		
		public readonly Parts.PlayablePartInfo Playable;

		public readonly PartInfo[] PartInfos;

		public ActorType(TextureInfo idle, int idleFacings, TextureInfo walk, int walkFacings, TextureInfo attack, int attackFacings, TextureInfo shadow, CPos offset, float scale, int height, int mana, int physicalSize, Shape physicalShape, WeaponType manaWeapon, int manaCost, Parts.PlayablePartInfo playable, PartInfo[] partInfos)
		{
			Idle = idle;
			IdleFacings = idleFacings;
			Walk = walk;
			WalkFacings = walkFacings;
			Attack = attack;
			AttackFacings = attackFacings;
			Shadow = shadow;

			Offset = offset;
			Scale = scale;
			Height = height;

			PhysicalSize = physicalSize;
			PhysicalShape = physicalShape;

			Mana = mana;
			ManaCost = manaCost;
			ManaWeapon = manaWeapon;

			Playable = playable;
			PartInfos = partInfos;
		}
	}

}
