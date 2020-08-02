using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons;

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

		public WeaponPartInfo(string internalName, MiniTextNode[] nodes) : base(internalName, nodes)
		{

		}
	}

	public class WeaponPart : ActorPart
	{
		readonly WeaponPartInfo info;
		public readonly WeaponType Type;

		public CPos WeaponOffsetPosition => self.GraphicPositionWithoutHeight + info.Offset;
		public int WeaponHeightPosition => self.Height + info.Height;

		public CPos Target;
		public int TargetHeight;

		BeamWeapon beam;

		bool attackOrdered;
		Target target;
		int prep;
		int post;

		public WeaponPart(Actor self, WeaponPartInfo info) : base(self)
		{
			this.info = info;
			Type = info.Type;
		}

		public override void OnLoad(List<MiniTextNode> nodes)
		{
			foreach (var node in nodes.Where(n => n.Key == "WeaponPart" && n.Value == info.InternalName))
			{
				if (node.Key == "BeamWeapon")
				{
					var id = node.Convert<int>();
					beam = (BeamWeapon)self.World.WeaponLayer.Weapons.FirstOrDefault(w => w.ID == id);
				}
			}
		}

		public override PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			if (beam != null)
				saver.Add("BeamWeapon", beam.ID, -1);
			
			return saver;
		}

		public void OnAttack(Target target)
		{
			if (attackOrdered)
				return;

			attackOrdered = true;
			this.target = target;
			prep = Type.PreparationDelay;
		}

		public override void Tick()
		{
			if (self.World.Game.Editor)
				return;

			if (attackOrdered && prep-- <= 0)
				attack();

			if (prep > 0 || post-- > 0 || (beam != null && !beam.Disposed))
				self.CurrentAction = ActorAction.ATTACKING;

			if (beam != null)
			{
				if (beam.Disposed)
				{
					beam = null;
					return;
				}

				beam.Move(Target, TargetHeight);
			}
		}

		void attack()
		{
			attackOrdered = false;
			post = Type.CooldownDelay;

			var weapon = WeaponCreator.Create(self.World, info.Type, target, self);
			Target = weapon.TargetPosition;
			beam = weapon as BeamWeapon;

			self.World.Add(weapon);
			self.AttackWith(target, weapon);
		}

		public override void OnDispose()
		{
			if (beam != null && !beam.Disposed)
				beam.Dispose();
		}
	}
}
