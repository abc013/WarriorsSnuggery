using System.Linq;
using WarriorsSnuggery.Audio.Sound;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Weapons
{
	public abstract class Weapon : PositionableObject, ILoadable, ISaveable
	{
		protected readonly World World;
		[Save]
		public override CPos Position { get => base.Position; set => base.Position = value; }

		public readonly uint ID;

		// Saved separately
		public Actor Origin { get; private set; }
		[Save]
		public readonly byte Team;

		[Save]
		public float Angle;
		[Save]
		public int DistanceTravelled;

		// Saved separately
		public Target Target;
		[Save]
		public CPos TargetPosition;

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
		}

		public virtual void Load(TextNodeInitializer initializer)
		{
			var originID = initializer.Convert(nameof(Origin), uint.MaxValue);
			Origin = World.ActorLayer.ToAdd().FirstOrDefault(a => a.ID == originID);

			Target = new Target(initializer.MakeInitializerWith(nameof(Target)), World);

			initializer.SetSaveFields(this);
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

		public virtual TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			saver.AddSaveFields(this);
			if (Origin != null)
				saver.Add(nameof(Origin), Origin.ID);

			saver.AddChildren(nameof(Target), Target.Save());

			return saver;
		}
	}
}
