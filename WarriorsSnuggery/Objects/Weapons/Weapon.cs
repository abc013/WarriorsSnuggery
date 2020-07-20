using System.Linq;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects.Weapons
{
	public abstract class Weapon : PhysicsObject
	{
		protected readonly World World;

		public readonly Actor Origin;
		public Target Target;
		public CPos TargetPosition;
		public int TargetHeight;

		protected Actor TargetActor;

		protected readonly WeaponType Type;

		public readonly float InaccuracyModifier = 1f;
		public readonly float DamageModifier = 1f;
		public readonly float DamageRangeModifier = 1f;
		public readonly float RangeModifier = 1f;

		protected Weapon(World world, WeaponType type, Target target, Actor origin) : base(origin.ActiveWeapon != null ? origin.ActiveWeapon.WeaponOffsetPosition : origin.GraphicPosition, type.Projectile.GetTexture(), SimplePhysics.Empty)
		{
			World = world;
			Type = type;
			Origin = origin;

			Height = origin.ActiveWeapon != null ? origin.ActiveWeapon.WeaponHeightPosition : origin.Height;

			Target = target;
			TargetPosition = target.Position;
			TargetHeight = target.Height;

			if (origin != null)
			{
				var effects = origin.Effects.Where(e => e.Active);

				foreach (var effect in effects.Where(e => e.Spell.Type == Spells.EffectType.INACCURACY))
					InaccuracyModifier *= effect.Spell.Value;

				foreach (var effect in effects.Where(e => e.Spell.Type == Spells.EffectType.DAMAGE))
					DamageModifier *= effect.Spell.Value;

				foreach (var effect in effects.Where(e => e.Spell.Type == Spells.EffectType.DAMAGERANGE))
					DamageRangeModifier *= effect.Spell.Value;

				foreach (var effect in effects.Where(e => e.Spell.Type == Spells.EffectType.RANGE))
					RangeModifier *= effect.Spell.Value;
			}

			if (Type.FireSound != null)
			{
				var sound = new Sound(Type.FireSound);
				sound.Play(Position, false);
			}
		}

		public override void Tick()
		{
			base.Tick();

			if (!World.Game.Editor && InRange(TargetPosition))
				Detonate(new Target(TargetPosition, Height));
		}

		public override void Render()
		{
			RenderShadow();
			base.Render();
		}

		public virtual bool InRange(CPos position, int range = 128)
		{
			return (Position - position).SquaredFlatDist <= range * range;
		}

		public virtual void Detonate(Target finalTarget, bool dispose = true, bool detonateOnce = false)
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
	}
}
