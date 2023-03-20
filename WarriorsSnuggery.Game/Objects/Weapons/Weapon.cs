using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Audio.Sound;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Weapons
{
	public abstract class Weapon : PositionableObject
	{
		protected readonly World World;

		public readonly uint ID;

		[Save, DefaultValue(null)]
		public readonly Actor Origin;
		[Save]
		public readonly byte Team;

		[Save]
		public float Angle;
		[Save]
		public int DistanceTravelled;

		public Target Target;
		[Save]
		public CPos TargetPosition;
		[Save]
		public int TargetHeight;

		[Save]
		protected readonly WeaponType Type;

		[Save]
		public readonly float InaccuracyModifier = 1f;
		[Save]
		public readonly float DamageModifier = 1f;
		[Save]
		public readonly float DamageRangeModifier = 1f;
		[Save]
		public readonly float RangeModifier = 1f;

		protected Weapon(World world, WeaponType type, Target target, Actor origin, uint id)
		{
			Renderable = type.Projectile.GetTexture();
			World = world;
			Type = type;

			Target = target;
			TargetPosition = target.Position;

			Origin = origin;
			Team = origin == null ? Actor.NeutralTeam : origin.Team;

			Position = origin.Weapon != null ? origin.Weapon.WeaponOffsetPosition : origin.GraphicPosition;

			ID = id;

			foreach (var effect in origin.GetActiveEffects(EffectType.INACCURACY))
				InaccuracyModifier *= effect.Effect.Value;

			foreach (var effect in origin.GetActiveEffects(EffectType.DAMAGE))
				DamageModifier *= effect.Effect.Value;

			foreach (var effect in origin.GetActiveEffects(EffectType.DAMAGERANGE))
				DamageRangeModifier *= effect.Effect.Value;

			foreach (var effect in origin.GetActiveEffects(EffectType.RANGE))
				RangeModifier *= effect.Effect.Value;

			if (Type.FireSound != null)
			{
				var sound = new Sound(Type.FireSound);
				sound.Play(Position, false);
			}
		}

		protected Weapon(World world, WeaponInit init)
		{
			Renderable = init.Type.Projectile.GetTexture();
			World = world;
			Type = init.Type;
			ID = init.ID;

			Position = init.Position;

			var originID = init.Convert("Origin", uint.MaxValue);
			Origin = world.ActorLayer.ToAdd().FirstOrDefault(a => a.ID == originID);

			var targetID = init.Convert("TargetActor", uint.MaxValue);
			var TargetActor = world.ActorLayer.ToAdd().FirstOrDefault(a => a.ID == targetID);

			if (TargetActor == null)
			{
				var targetPos = init.Convert("OriginalTargetPosition", CPos.Zero);
				Target = new Target(targetPos);
			}
			else
				Target = new Target(TargetActor);

			Angle = init.Convert("Angle", 0f);
			DistanceTravelled = init.Convert("DistanceTravelled", 0);

			TargetPosition = init.Convert("TargetPosition", CPos.Zero);
			
			Team = init.Convert("Team", Team);

			InaccuracyModifier = init.Convert("InaccuracyModifier", InaccuracyModifier);
			DamageModifier = init.Convert("DamageModifier", DamageModifier);
			DamageRangeModifier = init.Convert("DamageRangeModifier", DamageRangeModifier);
			RangeModifier = init.Convert("RangeModifier", RangeModifier);
		}

		public override void Tick()
		{
			base.Tick();

			if (!World.IsInWorld(Position) || !World.Game.Editor && DistanceTravelled >= Type.MaxRange * RangeModifier)
				Detonate(new Target(Position));
		}

		public override void Render()
		{
			if (Type.ShowShadow)
				RenderShadow();

			base.Render();
		}

		protected CPos clampToMaxRange(CPos origin, float angle)
		{
			var maxRadius = Type.MaxRange * RangeModifier;

			return origin + CPos.FromFlatAngle(angle, maxRadius);
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
			var list = SaveAttribute.GetFields(this);

			if (Target.Type == TargetType.ACTOR)
				list.Add("TargetActor=" + Target.Actor.ID);

			list.Add("OriginalTargetPosition=" + Target.Position);

			return list;
		}
	}
}
