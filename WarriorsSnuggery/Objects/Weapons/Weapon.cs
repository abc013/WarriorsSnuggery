using System;
using System.Collections.Generic;
using System.Linq;

namespace WarriorsSnuggery.Objects.Weapons
{
	public abstract class Weapon : PhysicsObject
	{
		protected readonly World World;

		public readonly uint ID;

		public readonly Actor Origin;
		public readonly byte Team;

		public float Angle;
		public int DistanceTravelled;

		public Target Target;
		public CPos TargetPosition;
		public int TargetHeight;

		protected readonly WeaponType Type;

		public readonly float InaccuracyModifier = 1f;
		public readonly float DamageModifier = 1f;
		public readonly float DamageRangeModifier = 1f;
		public readonly float RangeModifier = 1f;

		protected Weapon(World world, WeaponType type, Target target, Actor origin, uint id) : base(origin.ActiveWeapon != null ? origin.ActiveWeapon.WeaponOffsetPosition : origin.GraphicPosition, type.Projectile.GetTexture())
		{
			World = world;
			Type = type;

			Target = target;
			TargetPosition = target.Position;
			TargetHeight = target.Height;

			Origin = origin;
			Team = origin == null ? Actor.NeutralTeam : origin.Team;

			ID = id;

			Height = origin.ActiveWeapon != null ? origin.ActiveWeapon.WeaponHeightPosition : origin.Height;

			var effects = origin.Effects.Where(e => e.Active);

			foreach (var effect in effects.Where(e => e.Spell.Type == Spells.EffectType.INACCURACY))
				InaccuracyModifier *= effect.Spell.Value;

			foreach (var effect in effects.Where(e => e.Spell.Type == Spells.EffectType.DAMAGE))
				DamageModifier *= effect.Spell.Value;

			foreach (var effect in effects.Where(e => e.Spell.Type == Spells.EffectType.DAMAGERANGE))
				DamageRangeModifier *= effect.Spell.Value;

			foreach (var effect in effects.Where(e => e.Spell.Type == Spells.EffectType.RANGE))
				RangeModifier *= effect.Spell.Value;

			if (Type.FireSound != null)
			{
				var sound = new Sound(Type.FireSound);
				sound.Play(Position, false);
			}
		}

		protected Weapon(World world, WeaponInit init) : base(init.Position, init.Type.Projectile.GetTexture())
		{
			World = world;
			Type = init.Type;
			ID = init.ID;

			Height = init.Height;

			var originID = init.Convert("Origin", uint.MaxValue);
			Origin = world.ActorLayer.ToAdd().FirstOrDefault(a => a.ID == originID);

			var targetID = init.Convert("TargetActor", uint.MaxValue);
			var TargetActor = world.ActorLayer.ToAdd().FirstOrDefault(a => a.ID == targetID);

			if (TargetActor == null)
			{
				var targetPos = init.Convert("OriginalTargetPosition", CPos.Zero);
				var targetHeight = init.Convert("OriginalTargetHeight", 0);
				Target = new Target(targetPos, targetHeight);
			}
			else
				Target = new Target(TargetActor);

			Angle = init.Convert("Angle", 0f);
			DistanceTravelled = init.Convert("DistanceTravelled", 0);

			TargetPosition = init.Convert("TargetPosition", CPos.Zero);
			TargetHeight = init.Convert("TargetHeight", 0);
			
			Team = init.Convert("Team", Team);

			InaccuracyModifier = init.Convert("InaccuracyModifier", InaccuracyModifier);
			DamageModifier = init.Convert("DamageModifier", DamageModifier);
			DamageRangeModifier = init.Convert("DamageRangeModifier", DamageRangeModifier);
			RangeModifier = init.Convert("RangeModifier", RangeModifier);
		}

		public override void Tick()
		{
			base.Tick();

			if (Height < 0 || !World.IsInWorld(Position))
				Detonate(new Target(Position, 0));

			if (!World.Game.Editor &&  DistanceTravelled >= Type.MaxRange)
				Detonate(new Target(Position, Height));
		}

		public override void Render()
		{
			RenderShadow();
			base.Render();
		}

		protected CPos clampToMaxRange(CPos origin, float angle)
		{
			var maxRadius = Type.MaxRange * RangeModifier;

			var x = (int)(Math.Cos(angle) * maxRadius);
			var y = (int)(Math.Sin(angle) * maxRadius);

			return origin + new CPos(x, y, 0);
		}

		protected CPos getInaccuracy(int inaccuracy)
		{
			if (inaccuracy <= 0)
				return CPos.Zero;

			var random = World.Game.SharedRandom;
			var x = (int)(random.Next(-inaccuracy, inaccuracy) * InaccuracyModifier);
			var y = (int)(random.Next(-inaccuracy, inaccuracy) * InaccuracyModifier);

			return new CPos(x, y, 0);
		}

		public virtual bool InRange(CPos position, int range = 128)
		{
			return (Position - position).SquaredFlatDist <= range * range;
		}

		public virtual void Detonate(Target finalTarget, bool dispose = true)
		{
			if (Disposed)
				return;

			foreach (var warhead in Type.Warheads)
				warhead.Impact(World, this, finalTarget);

			if (dispose)
				Dispose();
		}

		public override void Dispose()
		{
			base.Dispose();

			World.WeaponLayer.Remove(this);
		}

		public virtual List<string> Save()
		{
			var list = new List<string>
			{
				"Position=" + Position,
				"Height=" + Height,
				"Type=" +  WeaponCreator.Types.FirstOrDefault(t => t.Value == Type).Key,
				"Team=" + Team,
				"InaccuracyModifier=" + InaccuracyModifier,
				"DamageModifier=" + DamageModifier,
				"DamageRangeModifier=" + DamageRangeModifier,
				"RangeModifier=" + RangeModifier
			};
			if (Origin != null)
				list.Add("Origin=" + Origin.ID);
			if (Target.Type == TargetType.ACTOR)
				list.Add("TargetActor=" + Target.Actor.ID);

			if (Angle != 0)
				list.Add("Angle=" + Angle);
			if (DistanceTravelled != 0)
				list.Add("DistanceTravelled=" + DistanceTravelled);

			list.Add("OriginalTargetPosition=" + Target.Position);
			list.Add("OriginalTargetHeight=" + Target.Height);
			list.Add("TargetPosition=" + TargetPosition);
			list.Add("TargetHeight=" + TargetHeight);

			return list;
		}
	}
}
