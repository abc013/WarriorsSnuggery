﻿using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
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

		public WeaponPartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new WeaponPart(self, this);
		}
	}

	public class WeaponPart : ActorPart, ITick, INoticeDispose
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

		public int Reload;
		public bool ReloadDone => Reload <= 0;

		public WeaponPart(Actor self, WeaponPartInfo info) : base(self)
		{
			this.info = info;
			Type = info.Type;
		}

		public override void OnLoad(List<TextNode> nodes)
		{
			foreach (var node in nodes.Where(n => n.Key == "WeaponPart" && n.Value == info.InternalName))
			{
				if (node.Key == "BeamWeapon")
				{
					var id = node.Convert<int>();
					beam = (BeamWeapon)self.World.WeaponLayer.Weapons.FirstOrDefault(w => w.ID == id);
				}
				else if (node.Key == "PreparationTick")
					prep = node.Convert<int>();
				else if (node.Key == nameof(Reload))
					Reload = node.Convert<int>();
				else if (node.Key == "Target")
				{
					var pos = node.Convert<CPos>();
					target = new Target(new CPos(pos.X, pos.Y, 0), pos.Z);
				}
			}
		}

		public override PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			if (beam != null)
				saver.Add("BeamWeapon", beam.ID, -1);

			saver.Add("PreparationTick", prep, 0);
			saver.Add(nameof(Reload), Reload, 0);

			if (target != null)
			{
				// TODO: also support actor targeting
				saver.Add("Target", target.Position + new CPos(0, 0, target.Height), CPos.Zero);
			}
			
			return saver;
		}

		public void OnAttack(Target target)
		{
			attackOrdered = true;
			this.target = target;

			if (Type.PreparationDelay != 0)
				prep = Type.PreparationDelay;
			else
				attack();
		}

		public void Tick()
		{
			Reload--;

			if (attackOrdered && prep-- <= 0)
				attack();

			if (beam != null)
			{
				if (beam.Disposed)
					beam = null;
				else
					beam.Move(Target, TargetHeight);
			}
		}

		void attack()
		{
			attackOrdered = false;

			if (Type.PreparationDelay != 0 && self.CurrentAction.Type != ActionType.PREPARE_ATTACK)
				return;

			var weapon = WeaponCreator.Create(self.World, info.Type, target, self);
			Target = weapon.TargetPosition;
			beam = weapon as BeamWeapon;

			if (self.AttackWith(target, weapon))
			{
				var reloadModifier = 1f;
				foreach (var effect in self.Effects.Where(e => e.Active && e.Effect.Type == Spells.EffectType.COOLDOWN))
					reloadModifier *= effect.Effect.Value;

				Reload = (int)(Type.Reload * reloadModifier);
			}
		}

		public void OnDispose()
		{
			if (beam != null && !beam.Disposed)
				beam.Dispose();
		}
	}
}
