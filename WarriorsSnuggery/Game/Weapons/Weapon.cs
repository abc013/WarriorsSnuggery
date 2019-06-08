using System;
using System.Linq;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Weapon : PhysicsObject
	{
		protected readonly World World;
		protected readonly Actor Origin;
		public CPos Target;
		protected Actor TargetActor;
		protected readonly WeaponType Type;
		protected float DistanceMoved;
		protected int Speed;

		readonly float inaccuracyModifier = 1f;
		readonly float damageModifier = 1f;
		readonly float rangeModifier = 1f;

		public Weapon(World world, WeaponType type, Actor origin, CPos target) : this(world, type, origin.ActiveWeapon.WeaponOffsetPosition, target, origin)
		{
		}

		public Weapon(World world, WeaponType type, CPos origin, CPos target, Actor originActor = null) : base(origin, new SpriteRenderable(type.Textures.GetTextures(), 1f, type.Textures.Tick), new Physics(origin, 0, type.PhysicalShape, type.PhysicalSize, type.PhysicalSize, type.PhysicalSize))
		{
			World = world;
			Type = type;
			Origin = originActor;
			if (type.Acceleration == 0)
				Speed = type.Speed;

			Target = target;

			if (Type.OrientateToTarget)
				Rotation = new CPos(0,0, (int) -Position.AngleToXY(Target) + 90);

			if (originActor != null)
			{
				foreach (var effect in originActor.Effects.Where(e => e.Active && e.Effect.Type == EffectType.INACCURACY))
				{
					inaccuracyModifier *= effect.Effect.Value;
				}

				foreach (var effect in originActor.Effects.Where(e => e.Active && e.Effect.Type == EffectType.DAMAGE))
				{
					damageModifier *= effect.Effect.Value;
				}

				foreach (var effect in originActor.Effects.Where(e => e.Active && e.Effect.Type == EffectType.RANGE))
				{
					rangeModifier *= effect.Effect.Value;
				}
			}
		}

		public override void Tick()
		{
			base.Tick();

			if (Type.Acceleration != 0 && Speed != Type.Speed)
			{
				Speed += Type.Acceleration;
				if (Speed > Type.Speed)
					Speed = Type.Speed;
			}

			Move(Target);

			if (Type.OrientateToTarget)
				Rotation = new CPos(0,0, (int) -Position.AngleToXY(Target) + 90);

			if (InRange(Target))
				Detonate();
		}

		public virtual void Move(CPos target)
		{
			var angle = target.AngleToXY(Position);

			var x = Math.Cos((angle * Math.PI) / 180) * Speed;
			var y = Math.Sin((angle * Math.PI) / 180) * Speed;

			var old = Position;
			// Note: we made sure that a weapon's target can't be out of world. (Actor.cs#87(Attack))
			Position = new CPos(Position.X + (int) x, Position.Y + (int) y, Position.Z);
			Physics.Position = Position;
			World.PhysicsLayer.UpdateSectors(this);

			if (World.CheckCollision(this, true, new [] { typeof(Weapon), typeof(BeamWeapon), typeof(BulletWeapon), typeof(RocketWeapon) }, new[] { Origin }))
				Detonate();

			DistanceMoved += Position.DistToXY(old);
			if (DistanceMoved > Type.MaxRange * rangeModifier || !World.IsInWorld(Position))
				Detonate();
		}

		public virtual bool InRange(CPos position, int range = 256)
		{
			return Position.DistToXY(position) <= range;
		}

		public virtual void Detonate()
		{
			foreach(var actor in World.Actors)
			{
				if (Origin != null && Origin.Team == actor.Team)
					continue;

				var dist = Position.DistToXY(actor.Position) / 512;
				if (dist > 512f) continue;
				if (dist < 1f) dist = 1;

				float damagemultiplier = 0f;

				switch(Type.DamageFalloff)
				{
					case FalloffType.QUADRATIC:
						damagemultiplier = 1 / (float) (dist * dist);
						break;
					case FalloffType.CUBIC:
						damagemultiplier = 1 / (float) (dist * dist * dist);
						break;
					case FalloffType.EXPONENTIAL:
						damagemultiplier = 1 / (float) Math.Pow(5, dist);
						break;
					case FalloffType.LINEAR:
						damagemultiplier = 1 / (float) dist;
						break;
					case FalloffType.ROOT:
						damagemultiplier = 1 / (float) Math.Sqrt(dist);
						break;
				}
				var damage = (int) Math.Floor(damagemultiplier * Type.Damage * damageModifier);

				if (damage < 2 || !actor.IsAlive)
					continue;

				if (actor.WorldPart != null && actor.WorldPart.ShowDamage)
					World.Add(new ActionText(actor.Position + new CPos(0,0,1024), IFont.Pixel16, new CPos(0, -15, 30), 50, new Color(1f, 1 - (damage / (Type.Damage * 1.5f)), 0).ToString() + damage.ToString()));

				if (Origin != null)
					actor.Damage(Origin, damage);
				else
					actor.Damage(damage);
			}

			if (Type.Smudge != null && World.TerrainAt(Position) != null && World.TerrainAt(Position).Type.SpawnSmudge)
				World.Add(new Smudge(new CPos(Position.X, Position.Y, -512), new SpriteRenderable(Type.Smudge.GetTextures(), tick: Type.Smudge.Tick)));

			if (Type.ParticlesOnImpact != null)
			{
				foreach(var particle in Type.ParticlesOnImpact.Create(Position))
				{
					World.Add(particle);
				}
			}

			Dispose();
		}

		protected CPos getInaccuracy()
		{
			if (Type.Inaccuracy > 0)
			{
				var ranX = (Program.SharedRandom.Next(Type.Inaccuracy) - Type.Inaccuracy / 2) * inaccuracyModifier;
				var ranY = (Program.SharedRandom.Next(Type.Inaccuracy) - Type.Inaccuracy / 2) * inaccuracyModifier;

				return new CPos((int) ranX, (int) ranY, 0);
			}

			return CPos.Zero;
		}
	}
}
